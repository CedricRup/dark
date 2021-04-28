The solution si currently based on https://github.com/Tewr/BlazorWorker

This library allow full interop with Blazor following this path: 

 (Blazor main app -> javascript -> Blazor Web Worker -> javascript -> Blazor) .
 
 All of it may not be required for Dark because we can skip steprs
 
 javascript ->Blazor Web worker -> javascript)
 
 The solution can be simplified at the cost of flexibility

What remains to do:

- Integrate in Dark client app. 
   Only the js script and the _framework folder in Wasm build result are required
- 2 incompabilities with existing code to investigate (see specific commit)
  - implementation of delay for taskv
  - error when calling state.tracing.storeFnResult
- decide to get rid of Tewr to simplify or keep it for flexibility    
- Implement correct serialization to communicate with webworker through messages to
  - pass eval parameters from client to worker
  - pass result from worker to site 


dotnet run in this folder and push the button for a demo