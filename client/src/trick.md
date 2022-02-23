If you want to persist a map of connectionId and for example userId.

```js
public void Subscribe(bool isChat) {
    string user = Context.User.Identity.Name;
    if (isChat){
        _chatConnections.Add(user, Context.ConnectionId);
    } else{
        _otherConnections.Add(user, Context.ConnectionId);
    }
}
```

You call this method after the hub is connected. It is more flexible in terms that it is then possible to change the notification type without having to reconnect. (Unsubscribe and Subscribe)

Alternative

If you don't want the extra roundtrip/flexibility. You can send QueryString parameters when connecting to the hub. Stackoverflow answer: Signalr persistent connection with query params.

 ```$.connection.hub.qs = 'isChat=true';```
