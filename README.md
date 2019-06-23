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

To publish a message, use the ```Send``` function

``` c#
Send<T>(T message) where T : IMessage
...
messenger.Send(new RequestDialogMessage());
```

In the above example, the ```RequestDialogMessage``` type is your own custom type, inheriting either the abstract ```Message``` class or the ```IMessage``` interface.
All types implementing ```IMessage``` have optional sender, id and tag fields.
