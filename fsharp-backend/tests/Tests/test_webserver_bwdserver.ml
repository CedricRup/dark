let t_should_use_https () =
  let custom_domain = "https.customdomain.com" in
  let canvas = "test" in
  Db.run
    ~name:"insert test custom_domain"
    "INSERT INTO custom_domains(host,canvas) VALUES ($1, $2)"
    ~params:[Db.String custom_domain; Db.String canvas] ;
  AT.check
    (AT.list AT.bool)
    "should_use_https works"
    (List.map
       ~f:(fun x -> Webserver.should_use_https (Uri.of_string x))
       [ "http://builtwithdark.com"
       ; "http://test.builtwithdark.com"
       ; "http://localhost"
       ; "http://test.localhost"
       ; "http://" ^ custom_domain ])
    [true; true; false; false; true]


let t_route_host () =
  let custom_domain = "route_host.customdomain.com" in
  let canvas = "test-route_host" in
  let open Libbackend.Webserver in
  Db.run
    ~name:"insert test custom_domain"
    "INSERT INTO custom_domains(host,canvas) VALUES ($1, $2)"
    ~params:[Db.String custom_domain; Db.String canvas] ;
  AT.check
    (AT.list AT.string)
    "route_host works"
    (* foo checks
     * *.builtwithdark.com;  custom_domain checks routing via the db's
     * custom_domains table *)
    ["foo"; "test-route_host"]
    ( ["http://foo.builtwithdark.com"; "http://" ^ custom_domain]
    |> List.map ~f:Uri.of_string
    |> List.map ~f:CRequest.make
    |> List.map ~f:route_host
    |> List.map ~f:(function
           | None | Some Static | Some Admin ->
               "failure"
           | Some (Canvas canvas) ->
               canvas) )


let t_redirect_to () =
  AT.check
    (AT.list (AT.option AT.string))
    "redirect_to works"
    (List.map
       ~f:(fun x ->
         x
         |> Uri.of_string
         |> Webserver.redirect_to
         |> Option.map ~f:Uri.to_string)
       [ "http://example.com"
       ; "http://builtwithdark.com"
       ; "https://builtwithdark.com"
       ; "http://test.builtwithdark.com"
       ; "https://test.builtwithdark.com"
       ; "http://test.builtwithdark.com/x/y?z=a" ])
    [ None
    ; Some "https://builtwithdark.com"
    ; None
    ; Some "https://test.builtwithdark.com"
    ; None
    ; Some "https://test.builtwithdark.com/x/y?z=a" ]


let t_canonicalize_maintains_schemes () =
  (* We don't test the https variants as the request we can't make requests
   * that have them, and they never occur in either dev or prod. *)
  let tests =
    [ ( "//example.com"
      , ("http://example.com/", "http://example.com/", "https://example.com/")
      )
    ; ( "//builtwithdark.com"
      , ( "http://builtwithdark.com/"
        , "http://builtwithdark.com/"
        , "https://builtwithdark.com/" ) )
    ; ( "http://builtwithdark.com"
      , ( "http://builtwithdark.com/"
        , "http://builtwithdark.com/"
        , "https://builtwithdark.com/" ) )
    ; ( "http://test.builtwithdark.com"
      , ( "http://test.builtwithdark.com/"
        , "http://test.builtwithdark.com/"
        , "https://test.builtwithdark.com/" ) )
    ; ( "http://test.builtwithdark.com/x/y?z=a"
      , ( "http://test.builtwithdark.com/x/y?z=a"
        , "http://test.builtwithdark.com/x/y?z=a"
        , "https://test.builtwithdark.com/x/y?z=a" ) ) ]
  in
  tests
  |> List.map ~f:(fun (url, (e1, e2, e3)) ->
         [ (url, Header.init (), e1)
         ; (url, Header.init_with "x-forwarded-proto" "http", e2)
         ; (url, Header.init_with "x-forwarded-proto" "https", e3) ])
  |> List.concat
  |> List.iter ~f:(fun (url, headers, expected) ->
         AT.check
           AT.string
           (url ^ " " ^ Header.to_string headers)
           expected
           ( Req.make ~meth:`GET ~headers (Uri.of_string url)
           |> Webserver.canonicalize_request
           |> Req.uri
           |> Uri.to_string ))


let t_sanitize_uri_path_with_repeated_slashes () =
  AT.check
    AT.string
    "/foo//bar->/foo/bar"
    (Webserver.sanitize_uri_path "/foo//bar")
    "/foo/bar"


let t_head_and_get_requests_are_coalesced () =
  let test_name = "head-and-get-requests-are-coalsced" in
  let setup_canvas () =
    let n1 = hop (http_handler (str "test_body")) in
    let canvas = ops2c_exn ("test-" ^ test_name) [n1] in
    C.save_all !canvas ;
    canvas
  in
  let respond_to_head_from_get (req : Req.t) : int * (int * string) =
    Lwt_main.run
      (let%lwt () = Nocrypto_entropy_lwt.initialize () in
       let test_id = Types.id_of_int 1234 in
       let canvas = setup_canvas () in
       let%lwt resp, body =
         Telemetry.with_root "test" (fun span ->
             Webserver.canvas_handler
               ~execution_id:test_id
               ~canvas:!canvas.host
               ~ip:""
               ~uri:(req |> Req.uri)
               ~body:""
               span
               req)
       in
       let code = resp |> Resp.status |> Code.code_of_status in
       let body_string = Cohttp_lwt__.Body.to_string body |> Lwt_main.run in
       resp
       |> Resp.headers
       |> (fun headers ->
            match Header.get headers "Content-Length" with
            | None ->
                0
            | Some h ->
                int_of_string h)
       |> fun content_length -> return (code, (content_length, body_string)))
  in
  let expected_body = "\"test_body\"" in
  let expected_content_length = String.length expected_body in
  AT.check
    (AT.list (AT.pair AT.int (AT.pair AT.int AT.string)))
    "canvas_handler returns same content-length for HEAD and GET requests"
    (List.map
       ~f:respond_to_head_from_get
       (* valid basic auth login on darklang.com *)
       [ Req.make
           ?meth:(Some `GET)
           (Uri.of_string
              ("http://" ^ test_name ^ ".builtwithdark.localhost:8000/test"))
         (* valid basic auth login on localhost *)
       ; Req.make
           ?meth:(Some `HEAD)
           (Uri.of_string
              ("http://" ^ test_name ^ ".builtwithdark.localhost:8000/test")) ])
    [ (200, (expected_content_length, expected_body))
    ; (200, (expected_content_length, "")) ]


let t_head_and_get_requests_are_coalesced () =
  let test_name = "head-and-get-requests-are-coalsced" in
  let setup_canvas () =
    let n1 = hop (http_handler (str "test_body")) in
    let canvas = ops2c_exn ("test-" ^ test_name) [n1] in
    C.save_all !canvas ;
    canvas
  in
  let respond_to_head_from_get (req : Req.t) : int * (int * string) =
    Lwt_main.run
      (let%lwt () = Nocrypto_entropy_lwt.initialize () in
       let test_id = Types.id_of_int 1234 in
       let canvas = setup_canvas () in
       let%lwt resp, body =
         Telemetry.with_root "test" (fun span ->
             Webserver.canvas_handler
               ~execution_id:test_id
               ~canvas:!canvas.host
               ~ip:""
               ~uri:(req |> Req.uri)
               ~body:""
               span
               req)
       in
       let code = resp |> Resp.status |> Code.code_of_status in
       let body_string = Cohttp_lwt__.Body.to_string body |> Lwt_main.run in
       resp
       |> Resp.headers
       |> (fun headers ->
            match Header.get headers "Content-Length" with
            | None ->
                0
            | Some h ->
                int_of_string h)
       |> fun content_length -> return (code, (content_length, body_string)))
  in
  let expected_body = "\"test_body\"" in
  let expected_content_length = String.length expected_body in
  AT.check
    (AT.list (AT.pair AT.int (AT.pair AT.int AT.string)))
    "canvas_handler returns same content-length for HEAD and GET requests"
    (List.map
       ~f:respond_to_head_from_get
       (* valid basic auth login on darklang.com *)
       [ Req.make
           ?meth:(Some `GET)
           (Uri.of_string
              ("http://" ^ test_name ^ ".builtwithdark.localhost:8000/test"))
         (* valid basic auth login on localhost *)
       ; Req.make
           ?meth:(Some `HEAD)
           (Uri.of_string
              ("http://" ^ test_name ^ ".builtwithdark.localhost:8000/test")) ])
    [ (200, (expected_content_length, expected_body))
    ; (200, (expected_content_length, "")) ]


let t_http_request_redirects () =
  let setup_canvas () =
    let n1 = hop (http_handler (str "test_body")) in
    let canvas = ops2c_exn "test" [n1] in
    C.save_all !canvas ;
    canvas
  in
  let respond (req : Req.t) : int =
    Lwt_main.run
      (let%lwt () = Nocrypto_entropy_lwt.initialize () in
       let test_id = Types.id_of_int 1234 in
       ignore (setup_canvas ()) ;
       let%lwt resp, body =
         Telemetry.with_root "test" (fun span ->
             Webserver.callback
               span
               ~k8s_callback:(fun _ ~execution_id ->
                 Cohttp_lwt_unix.Server.respond_string
                   ~status:(Cohttp.Code.status_of_code 911)
                   ~body:""
                   ())
               ""
               req
               ""
               test_id)
       in
       resp |> Resp.status |> Code.code_of_status |> return)
  in
  AT.check
    AT.int
    "http requests redirect"
    302
    (respond
       (Req.make
          ?meth:(Some `GET)
          (Uri.of_string "http://test.builtwithdark.com/test")))


let t_is_canvas_name_valid () =
  let is_valid valid name =
    AT.check
      AT.bool
      (name ^ " " ^ if valid then "passes" else "fails")
      valid
      (Webserver.is_canvas_name_valid name)
  in
  "demo-hello" |> is_valid true ;
  "demo-" |> is_valid false ;
  "demo--" |> is_valid false ;
  "demo" |> is_valid true ;
  "demo-hello-world" |> is_valid true ;
  "demo-hello_world" |> is_valid true ;
  "demo-hello world" |> is_valid false ;
  "-demo" |> is_valid false ;
  "demo-(^@^)" |> is_valid false ;
  "demo-a_a" |> is_valid true ;
  "demo-9" |> is_valid true ;
  ()


let t_is_service_name_valid () =
  let service =
    Telemetry.with_root "test" (fun span ->
        Telemetry.Span.log_params span
        |> List.find ~f:(fun (k, _) -> k = "service_name")
        |> Option.bind ~f:(function _, `String s -> Some s | _ -> None))
  in
  AT.check
    (AT.option AT.string)
    "service_name should be \"test\"."
    (Some "test")
    service
