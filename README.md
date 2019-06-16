# IntraMessaging
Allows messages to be queued and distributed within your app from a singleton class. Subscribers are given the choice of what they subscribe to.

### Installation
[nuget](https://www.nuget.org/packages/IntraMessaging)

```
Install-Package IntraMessaging
```

### Usage
All interaction with the IntraMessager takes place through the singleton class

```c#
IIntraMessager messager = IntraMessage.Instance;
```

To publish a message, use the ```Enqueue``` function

``` c#
Enqueue<T>(T message) where T : IMessage
...
messager.Enqueue(new RequestDialogMessage());
```

In the above example, the ```RequestDialogMessage``` type is your own custom type, inheriting either the abstract ```Message``` class or the ```IMessage``` interface.
All types inheriting ```IMessage``` have optional sender, id and tag fields.
