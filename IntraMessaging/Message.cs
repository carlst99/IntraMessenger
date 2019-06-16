﻿using System;

namespace IntraMessaging
{
    public abstract class Message
    {
        public object Sender { get; }
        public Guid Id { get; set; }
        public object Tag { get; set; }

        public Type SenderType => Sender.GetType();
    }
}