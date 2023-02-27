# `Watcher`

> Observe what `constellation` is doing.

Locally hostable control panel for `constellation`. Interacts with a storage account set through URL/ubikey to provide command and control.

## CORS

Just be aware on the storage account that we interact with. Both `blob` and `queue` must be [set separately](https://learn.microsoft.com/en-us/javascript/api/overview/azure/storage-queue-readme?view=azure-node-latest#cors) to allow CORS requests.

```
Allowed origins: *
Allowed verbs: DELETE,GET,HEAD,MERGE,POST,OPTIONS,PUT
Allowed headers: *
Exposed headers: *
Maximum age (seconds): 86400
```

