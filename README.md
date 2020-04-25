# IntraMessenger
Allows messages to be distributed within your app/projects from a singleton class, without tight coupling. Subscribers are given the choice of what they subscribe to.

**Note:** This project/package used to be called IntraMessaging

### Installation
Install from [nuget](https://www.nuget.org/packages/IntraMessenger)

```
Install-Package IntraMessenger
```

### Usage
All interaction with the Messenger takes place through the singleton class. You can also register this to your IoC container.

```c#
IMessenger messenger = Messenger.Instance;
```

**Subscribing/Unsubscribing**

To subscribe to the message delegate, use the `Messenger.Subscribe` method. There is an overload that supports asynchronous methods that return a `Task`. The `requestedMessageTypes` array tells the `Messenger` what types you want the `action` to be invoked for, when a `Send` method is called. All types must implement `IMessage`.

```c#
Guid Subscribe(Action<IMessage> action, Type[] requestedMessageTypes = null)
Guid Subscribe(Func<IMessage, Task> action, Type[] requestedMessageTypes = null)
```

To unsubscribe, call the `IntraMessenger.Unsubscribe` method and pass it the GUID key returned when the `Subscribe` method was called.

**Sending Messages**

To send a message, use the ```Send/SendAsync``` function:

``` c#
Send<T>(T message) where T : IMessage
...
messenger.Send(new RequestDialogMessage());
```

In the above example, the ```RequestDialogMessage``` type is your own custom type, implementing the ```IMessage``` interface.
