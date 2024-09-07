﻿using Baubit.Collections;
namespace Baubit.Aggregation.Test.Setup
{
    public class TestEvent
    {
        public int Id { get; private init; }
        public DateTime CreatedAt { get; private init; } = DateTime.Now;
        public DateTime PostedAt { get; set; }
        public ICollection<Receipt> Trace { get; init; } = new ConcurrentList<Receipt>();
        private static int idSeed = 1;

        public TestEvent()
        {
            Id = idSeed++;
        }
    }
}
