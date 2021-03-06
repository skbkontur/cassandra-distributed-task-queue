﻿using System;

namespace SkbKontur.Cassandra.DistributedTaskQueue.Tests.RemoteTaskQueue.RepositoriesTests
{
    internal static class Sugar
    {
        public static T With<T>(this T obj, Action<T> action)
        {
            action(obj);
            return obj;
        }
    }
}