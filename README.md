# IntraMessaging
Allows messages to be queued and distributed within your app from a singleton class. Subscribers are given the choice of what they subscribe to.

### Installation
Install from [nuget](https://www.nuget.org/packages/IntraMessaging)

```
Install-Package IntraMessaging
```

### Usage
All interaction with the IntraMessenger takes place through the singleton class

```c#
IIntraMessenger messenger = IntraMessenger.Instance;
```

**Subscribing/Unsubscribing**

To subscribe to the message stream, use the `IntraMessenger.Subscribe` method

```c#
/// <summary>
/// Creates a subscription to the message queue
/// </summary>
/// <param name="callback">The callback to invoke when a message is broadcast</param>
/// <param name="requestedMessageTypes">The types of message to subscribe to</param>
/// <returns>A GUID which can be used to unsubscribe from the message queue</returns>
Guid Subscribe(Action<IMessage> callback, Type[] requestedMessageTypes = null)
```

To unsubscribe, simply call the `IntraMessenger.Unsubscribe` method, passing it the GUID key returned when the `Subscribe` method was called

**Sending Messages**

To send a message, use the ```Send``` function

``` c#
Send<T>(T message) where T : IMessage
...
messenger.Send(new RequestDialogMessage());
```

In the above example, the ```RequestDialogMessage``` type is your own custom type, inheriting either the abstract ```Message``` class or the ```IMessage``` interface.
All types implementing ```IMessage``` have optional sender, id and tag fields.

**Modes**

`IntraMessenger` implements two modes:
- Mode.HeavySubscribe (default)
- Mode.HeavyMessaging

The heavy subscribe mode is more computationally expensive when performing subscribe/unsubscribe operations. However, it is faster to send messages in this mode. Also recommended if you have a large number of subscribers, as this will significantly affect the performance of the `HeavyMessaging` mode.
The heavy messaging mode is recommended when you are frequently subscribing/unsubscribing to the message stream. However, the benefit of this mode will start to decrease with large numbers of subscribers. Hence, `HeavySubscribe` is the default and recommended mode.

To change mode, use the `IntraMessenger.ChangeMode` method. It is recommended that you change modes at the beginning of your program's execution lifecycle, as the operation becomes more expensive proportional to the number of subscribers.

```c#
void ChangeMode(Mode changeTo)
```
