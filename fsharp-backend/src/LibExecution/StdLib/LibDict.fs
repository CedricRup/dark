module LibExecution.StdLib.LibDict

open System.Threading.Tasks
open FSharp.Control.Tasks
open LibExecution.RuntimeTypes
open FSharpPlus
open Prelude

let fn = FQFnName.stdlibFnName

let incorrectArgs = LibExecution.Errors.incorrectArgs

let varA = TVariable "a"
let varB = TVariable "b"


let fns : List<BuiltInFn> =
  [ { name = fn "Dict" "singleton" 0
      parameters = [ Param.make "key" TStr ""; Param.make "value" varA "" ]
      returnType = TDict varA
      description = "Returns a new dictionary with a single entry `key`: `value`."
      fn =
        (function
        | _, [ DStr k; v ] -> Value(DObj(Map.ofList [ (k, v) ]))
        | _ -> incorrectArgs ())
      sqlSpec = NotYetImplementedTODO
      previewable = Pure
      deprecated = NotDeprecated }
    { name = fn "Dict" "size" 0
      parameters = [Param.make "dict" (TDict varA) ""]
      returnType = TInt
      description =
        "Returns the number of entries in `dict` (the number of key-value pairs)."
      fn =
          (function
          | _, [DObj o] -> Value(DInt(bigint(Map.count o)))
          | _ -> incorrectArgs ())
      sqlSpec = NotYetImplementedTODO
      previewable = Pure
      deprecated = NotDeprecated }
    //   { name = fn "Dict" "keys" 0
    //   ; parameters = [Param.make "dict" TObj ""]
    //   ; returnType = TList
    //   ; description = "Returns `dict`'s keys in a list, in an arbitrary order."
    //   ; fn =
    //         (function
    //         | _, [DObj o] ->
    //             o
    //             |> DvalMap.keys
    //             |> List.map (fun k -> DStr k)
    //             |> fun l -> DList l
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    // ; { name = fn "Dict" "values" 0
    //   ; parameters = [Param.make "dict" TObj ""]
    //   ; returnType = TList
    //   ; description = "Returns `dict`'s values in a list, in an arbitrary order."
    //   ; fn =
    //         (function
    //         | _, [DObj o] -> DList (DvalMap.values o) | _ -> incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    // ; { name = fn "Dict" "toList" 0
    //   ; parameters = [Param.make "dict" TObj ""]
    //   ; returnType = TList
    //   ; description =
    //       "Returns `dict`'s entries as a list of `[key, value]` lists, in an arbitrary order. This function is the opposite of `Dict::fromList`."
    //   ; fn =
    //         (function
    //         | _, [DObj o] ->
    //             DvalMap.to_list o
    //             |> List.map (fun (k, v) ->
    //                    DList [DStr k; v])
    //             |> Dval.to_list
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    // ; { name = fn "Dict" "fromListOverwritingDuplicates" 0
    //   ; parameters = [Param.make "entries" TList ""]
    //   ; returnType = TObj
    //   ; description =
    //       "Returns a new dict with `entries`. Each value in `entries` must be a `[key, value]` list, where `key` is a `String`.
    //       If `entries` contains duplicate `key`s, the last entry with that key will be used in the resulting dictionary (use `Dict::fromList` if you want to enforce unique keys).
    //       This function is the opposite of `Dict::toList`."
    //   ; fn =
    //         (function
    //         | state, [DList l] ->
    //             let fold_fn
    //                 (idx : int)
    //                 (acc : (dval DvalMap.t, dval (* type error *)) result)
    //                 (dv : dval) : (dval DvalMap.t, dval (* type error *)) result =
    //               Result.bind acc (fun acc ->
    //                   match dv with
    //                   | DList [DStr k; value] ->
    //                       Ok
    //                         ( acc
    //                         |> DvalMap.insert
    //                              (Unicode_string.to_string k)
    //                              ~value )
    //                   | (DIncomplete _ | DErrorRail _ | DError _) as dv ->
    //                       Error dv
    //                   | v ->
    //                       let err_details =
    //                         match v with
    //                         | DList [k; _] ->
    //                             let tipe =
    //                               k
    //                               |> Dval.tipe_of
    //                               |> Dval.tipe_to_developer_repr_v0
    //                             in
    //                             Printf.sprintf
    //                               "Keys must be `String`s but the type of `%s` is `%s`."
    //                               (Dval.to_developer_repr_v0 k)
    //                               tipe
    //                         | DList l ->
    //                             Printf.sprintf
    //                               "It has length %i but must have length 2."
    //                               (List.length l)
    //                         | non_list ->
    //                             let tipe =
    //                               non_list
    //                               |> Dval.tipe_of
    //                               |> Dval.tipe_to_developer_repr_v0
    //                             in
    //                             Printf.sprintf
    //                               "It is of type `%s` instead of `List`."
    //                               tipe
    //                       in
    //                       Error
    //                         (DError
    //                            ( SourceNone
    //                            , Printf.sprintf
    //                                "Expected every value within the `entries` argument passed to `%s` to be a `[key, value]` list. However, that is not the case for the value at index %i: `%s`. %s"
    //                                state.executing_fnname
    //                                idx
    //                                (Dval.to_developer_repr_v0 v)
    //                                err_details )))
    //             in
    //             let result =
    //               l |> List.foldi (Ok DvalMap.empty) fold_fn
    //             in
    //             (match result with Ok res -> DObj res | Error v -> v)
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    // ; { name = fn "Dict" "fromList" 0
    //   ; parameters = [Param.make "entries" TList ""]
    //   ; returnType = TOption
    //   ; description =
    //       "Each value in `entries` must be a `[key, value]` list, where `key` is a `String`.
    //       If `entries` contains no duplicate keys, returns `Just dict` where `dict` has `entries`.
    //       Otherwise, returns `Nothing` (use `Dict::fromListOverwritingDuplicates` if you want to overwrite duplicate keys)."
    //   ; fn =
    //         (function
    //         | state, [DList l] ->
    //             let fold_fn
    //                 (idx : int) (acc : (dval DvalMap.t, dval) result) (dv : dval)
    //                 : (dval DvalMap.t, dval) result =
    //               (* The dval for the result error could either be [Error DError] (in case of a type error)
    //                * or an [Error (DOption OptNothing)] (in case there is a duplicate) *)
    //               Result.bind acc (fun acc ->
    //                   match dv with
    //                   | DList [DStr k; value] ->
    //                     ( match
    //                         DvalMap.insert_fail_override
    //                           (Unicode_string.to_string k)
    //                           ~value
    //                           acc
    //                       with
    //                     | `Ok dict ->
    //                         Ok dict
    //                     | `Duplicate ->
    //                         Error (DOption OptNothing) )
    //                   | (DIncomplete _ | DErrorRail _ | DError _) as dv ->
    //                       Error dv
    //                   | v ->
    //                       let err_details =
    //                         match v with
    //                         | DList [k; _] ->
    //                             let tipe =
    //                               k
    //                               |> Dval.tipe_of
    //                               |> Dval.tipe_to_developer_repr_v0
    //                             in
    //                             Printf.sprintf
    //                               "Keys must be `String`s but the type of `%s` `%s`."
    //                               (Dval.to_developer_repr_v0 k)
    //                               tipe
    //                         | DList l ->
    //                             Printf.sprintf
    //                               "It has length %i but must have length 2."
    //                               (List.length l)
    //                         | non_list ->
    //                             let tipe =
    //                               non_list
    //                               |> Dval.tipe_of
    //                               |> Dval.tipe_to_developer_repr_v0
    //                             in
    //                             Printf.sprintf
    //                               "It is of type `%s` instead of `List`."
    //                               tipe
    //                       in
    //                       Error
    //                         (DError
    //                            ( SourceNone
    //                            , Printf.sprintf
    //                                "Expected every value within the `entries` argument passed to `%s` to be a `[key, value]` list. However, that is not the case for the value at index %i: `%s`. %s"
    //                                state.executing_fnname
    //                                idx
    //                                (Dval.to_developer_repr_v0 v)
    //                                err_details )))
    //             in
    //             let result =
    //               l
    //               |> List.foldi (Ok DvalMap.empty) fold_fn
    //               |> Result.map (fun o -> DOption (OptJust (DObj o)))
    //             in
    //             (match result with Ok res -> res | Error v -> v)
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    { name = fn "Dict" "get" 0
      parameters = [ Param.make "dict" (TDict varA) ""; Param.make "key" TStr "" ]
      returnType = varA
      description =
        "Looks up `key` in object `dict` and returns the value if found, and Error otherwise"
      fn =
        (function
        | _, [ DObj o; DStr s ] ->
            (match Map.tryFind s o with
             | Some d -> Value(d)
             | None -> Value(DNull))
        | _ -> incorrectArgs ())
      sqlSpec = NotYetImplementedTODO
      previewable = Pure
      deprecated = ReplacedBy(fn "Dict" "get" 1) }
    { name = fn "Dict" "get" 1
      parameters = [ Param.make "dict" (TDict varA) ""; Param.make "key" TStr "" ]
      returnType = TOption varA
      description = "Looks up `key` in object `dict` and returns an option"
      fn =
        (function
        | _, [ DObj o; DStr s ] -> Value(DOption(Map.tryFind s o))
        | _ -> incorrectArgs ())
      sqlSpec = NotYetImplementedTODO
      previewable = Pure
      deprecated = ReplacedBy(fn "Dict" "get" 1) }
    // ; { name = fn "Dict" "get" 2
    //   ; parameters = [Param.make "dict" TObj ""; Param.make "key" TStr ""]
    //   ; returnType = TOption
    //   ; description =
    //       "If the `dict` contains `key`, returns the corresponding value, wrapped in an option: `Just value`. Otherwise, returns `Nothing`."
    //   ; fn =
    //         (function
    //         | _, [DObj o; DStr s] ->
    //           ( match DvalMap.get o (Unicode_string.to_string s) with
    //           | Some d ->
    //               Dval.to_opt_just d
    //           | None ->
    //               DOption OptNothing )
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    // ; { name = fn "Dict" "member" 0
    //   ; parameters = [Param.make "dict" TObj ""; Param.make "key" TStr ""]
    //   ; returnType = TBool
    //   ; description =
    //       "Returns `true` if the `dict` contains an entry with `key`, and `false` otherwise."
    //   ; fn =
    //         (function
    //         | _, [DObj o; DStr s] ->
    //             let key = Unicode_string.to_string s in
    //             DBool (DvalMap.contains_key o ~key)
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    // ; { name = fn "Dict" "foreach" 0
    //   ; parameters = [Param.make "dict" TObj ""; func ["val"]]
    //   ; returnType = TObj
    //   ; description =
    //       "Returns a new dictionary that contains the same keys as the original `dict` with values that have been transformed by `f`, which operates on each value."
    //   ; fn =
    //         (function
    //         | state, [DObj o; DFnVal b] ->
    //             let f dv = Ast.execute_dblock ~state b [dv] in
    //             DObj (Map.map ~f o)
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = ReplacedBy(fn "" "" 0) }
    // ; { name = fn "Dict" "map" 0
    //   ; parameters = [Param.make "dict" TObj ""; func ["key"; "value"]]
    //   ; returnType = TObj
    //   ; description =
    //       "Returns a new dictionary that contains the same keys as the original `dict` with values that have been transformed by `f`, which operates on each key-value pair.
    //       Consider `Dict::filterMap` if you also want to drop some of the entries."
    //   ; fn =
    //         (function
    //         | state, [DObj o; DFnVal b] ->
    //             let f ~key ~(data : dval) =
    //               Ast.execute_dblock ~state b [DStr key; data]
    //             in
    //             DObj (Map.mapi ~f o)
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    // ; { name = fn "Dict" "filter" 0
    //   ; parameters = [Param.make "dict" TObj ""; func ["key"; "value"]]
    //   ; returnType = TObj
    //   ; description =
    //       "Calls `f` on every entry in `dict`, returning a dictionary of only those entries for which `f key value` returns `true`.
    //       Consider `Dict::filterMap` if you also want to transform the entries."
    //   ; fn =
    //         (function
    //         | state, [DObj o; DFnVal b] ->
    //             let incomplete = ref false in
    //             let f ~(key : string) ~(data : dval) : bool =
    //               let result =
    //                 Ast.execute_dblock ~state b [DStr key; data]
    //               in
    //               match result with
    //               | DBool b ->
    //                   b
    //               | DIncomplete _ ->
    //                   incomplete := true ;
    //                   false
    //               | v ->
    //                   RT.error
    //                     "Expecting fn to return bool"
    //                     v
    //                     data
    //             in
    //             if !incomplete
    //             then DIncomplete SourceNone (*TODO(ds) source info *)
    //             else DObj (Base.Map.filteri ~f o)
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = ReplacedBy(fn "" "" 0) }
    // ; { name = fn "Dict" "filter" 1
    //   ; parameters = [Param.make "dict" TObj ""; func ["key"; "value"]]
    //   ; returnType = TObj
    //   ; description =
    //       "Evaluates `f key value` on every entry in `dict`. Returns a new dictionary that contains only the entries of `dict` for which `f` returned `true`."
    //   ; fn =
    //         (function
    //         | state, [DObj o; DFnVal b] ->
    //             let filter_propagating_errors ~key ~data acc =
    //               match acc with
    //               | Error dv ->
    //                   Error dv
    //               | Ok m ->
    //                   let result =
    //                     Ast.execute_dblock
    //                       ~state
    //                       b
    //                       [DStr key; data]
    //                   in
    //                   ( match result with
    //                   | DBool true ->
    //                       Ok (Base.Map.set m ~key ~data)
    //                   | DBool false ->
    //                       Ok m
    //                   | (DIncomplete _ as e) | (DError _ as e) ->
    //                       Error e
    //                   | other ->
    //                       RT.error
    //                         "Fn returned incorrect type"
    //                         "bool"
    //                         other )
    //             in
    //             let filtered_result =
    //               Base.Map.fold
    //                 o
    //                 (Ok DvalMap.empty)
    //                 filter_propagating_errors
    //             in
    //             (match filtered_result with Ok o -> DObj o | Error dv -> dv)
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    // ; { name = fn "Dict" "filterMap" 0
    //   ; parameters = [Param.make "dict" TObj ""; func ["key"; "value"]]
    //   ; returnType = TObj
    //   ; description =
    //       {|Calls `f` on every entry in `dict`, returning a new dictionary that drops some entries (filter) and transforms others (map).
    //       If `f key value` returns `Nothing`, does not add `key` or `value` to the new dictionary, dropping the entry.
    //       If `f key value` returns `Just newValue`, adds the entry `key`: `newValue` to the new dictionary.
    //       This function combines `Dict::filter` and `Dict::map`.|}
    //   ; fn =
    //         (function
    //         | state, [DObj o; DFnVal b] ->
    //             let abortReason = ref None in
    //             let f ~key ~(data : dval) : dval option =
    //               if !abortReason = None
    //               then (
    //                 match
    //                   Ast.execute_dblock
    //                     ~state
    //                     b
    //                     [DStr key; data]
    //                 with
    //                 | DOption (OptJust o) ->
    //                     Some o
    //                 | DOption OptNothing ->
    //                     None
    //                 | (DIncomplete _ | DErrorRail _ | DError _) as dv ->
    //                     abortReason := Some dv ;
    //                     None
    //                 | v ->
    //                     abortReason :=
    //                       Some
    //                         (DError
    //                            ( SourceNone
    //                            , "Expected the argument `f` passed to `"
    //                              ^ state.executing_fnname
    //                              ^ "` to return `Just` or `Nothing` for every entry in `dict`. However, it returned `"
    //                              ^ Dval.to_developer_repr_v0 v
    //                              ^ "` for the entry `"
    //                              ^ key
    //                              ^ " : "
    //                              ^ Dval.to_developer_repr_v0 data
    //                              ^ "`." )) ;
    //                     None )
    //               else None
    //             in
    //             let result = Map.filter_mapi ~f o in
    //             (match !abortReason with None -> DObj result | Some v -> v)
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    { name = fn "Dict" "empty" 0
      parameters = []
      returnType = TDict varA
      description = "Returns an empty dictionary."
      fn =
        (function
        | _, [] -> Value(DObj Map.empty)
        | _ -> incorrectArgs ())
      sqlSpec = NotQueryable
      previewable = Pure
      deprecated = NotDeprecated }
    // ; { name = fn "Dict" "isEmpty" 0
    //   ; parameters = [Param.make "dict" TObj ""]
    //   ; returnType = TBool
    //   ; description = "Returns `true` if the `dict` contains no entries."
    //   ; fn =
    //         (function
    //         | _, [DObj dict] -> DBool (DvalMap.is_empty dict) | _ -> incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    // ; { name = fn "Dict" "merge" 0
    //   ; parameters = [Param.make "left" TObj ""; Param.make "right" TObj ""]
    //   ; returnType = TObj
    //   ; description =
    //       "Returns a combined dictionary with both dictionaries' entries. If the same key exists in both `left` and `right`, it will have the value from `right`."
    //   ; fn =
    //         (function
    //         | _, [DObj l; DObj r] ->
    //             DObj (Stdlib_util.merge_right l r)
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    // ; { name = fn "Dict" "toJSON" 0
    //   ; parameters = [Param.make "dict" TObj ""]
    //   ; returnType = TStr
    //   ; description = "Returns `dict` as a JSON string."
    //   ; fn =
    //         (function
    //         | _, [DObj o] ->
    //             DObj o
    //             |> Dval.to_pretty_machine_json_v1
    //             |> DStr
    //         | _ -> args
    //             incorrectArgs ())
    //   ; sqlSpec = NotYetImplementedTODO
    //   ; previewable = Pure
    //   ; deprecated = NotDeprecated }
    { name = fn "Dict" "set" 0
      parameters =
        [ Param.make "dict" (TDict(TVariable "a")) ""
          Param.make "key" TStr ""
          Param.make "val" varA "" ]
      returnType = (TDict(TVariable "a"))
      description = "Returns a copy of `dict` with the `key` set to `val`."
      fn =
        (function
        | _, [ DObj o; DStr k; v ] -> Value(DObj(Map.add k v o))
        | _ -> incorrectArgs ())
      sqlSpec = NotYetImplementedTODO
      previewable = Pure
      deprecated = NotDeprecated }
    { name = fn "Dict" "remove" 0
      parameters = [ Param.make "dict" (TDict varA) ""; Param.make "key" TStr "" ]
      returnType = TDict varA
      description =
        "If the `dict` contains `key`, returns a copy of `dict` with `key` and its associated value removed. Otherwise, returns `dict` unchanged."
      fn =
        (function
        | _, [ DObj o; DStr k ] -> Value(DObj(Map.remove k o))
        | _ -> incorrectArgs ())
      sqlSpec = NotYetImplementedTODO
      previewable = Pure
      deprecated = NotDeprecated } ]
