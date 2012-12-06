﻿using System;

using ExchangeService.Repositories;
using ExchangeService.TaskDatas;

using RemoteQueue.Handling;
using RemoteQueue.Handling.HandlerResults;

namespace ExchangeService.TaskHandlers
{
    public class FakePeriodicTaskHandler : TaskHandler<FakePeriodicTaskData>
    {
        public FakePeriodicTaskHandler(ITestCounterRepository testCounterRepository)
        {
            this.testCounterRepository = testCounterRepository;
        }

        protected override HandleResult HandleTask(FakePeriodicTaskData taskData)
        {
            testCounterRepository.IncrementCounter(Context.Id);
            return Rerun(TimeSpan.FromSeconds(0.1));
        }

        private readonly ITestCounterRepository testCounterRepository;
    }
}