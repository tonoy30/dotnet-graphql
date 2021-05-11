﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConferencePlanner.Business.DbContexts;
using ConferencePlanner.Contacts.Models;
using GreenDonut;
using HotChocolate.DataLoader;
using Microsoft.EntityFrameworkCore;

namespace ConferencePlanner.Business.GraphQL.DataLoaders
{
    public class SessionByIdDataLoader : BatchDataLoader<int, Session>
    {
        private readonly IDbContextFactory<ApplicationDbContext> _dbContextFactory;

        public SessionByIdDataLoader(IBatchScheduler batchScheduler,
            IDbContextFactory<ApplicationDbContext> dbContextFactory) : base(batchScheduler)
        {
            _dbContextFactory = dbContextFactory ?? throw new ArgumentNullException(nameof(dbContextFactory));
        }

        protected override async Task<IReadOnlyDictionary<int, Session>> LoadBatchAsync(IReadOnlyList<int> keys,
            CancellationToken cancellationToken)
        {
            await using ApplicationDbContext dbContext = _dbContextFactory.CreateDbContext();
            return await dbContext.Sessions.Where(s => keys.Contains(s.Id))
                .ToDictionaryAsync(t => t.Id, cancellationToken);
        }
    }
}