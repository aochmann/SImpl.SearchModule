﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Examine;
using Microsoft.Extensions.Logging;
using SImpl.CQRS.Commands;
using SImpl.SearchModule.Abstraction.Commands;
using SImpl.SearchModule.Abstraction.Models;
using SImpl.SearchModule.Examine.Application.Services;
using SImpl.SearchModule.Examine.Configuration;
using SImpl.SearchModule.Examine.Models;

namespace SImpl.SearchModule.Examine.Application.CommandHandlers
{
    public class IndexCommandHandler : ICommandHandler<IndexCommand>
    {
        private readonly IExamineManager _examineManager;
        private readonly ExamineSearchConfiguration _configuration;
        private readonly ILogger<IndexCommandHandler> _logger;


        public IndexCommandHandler(IExamineManager examineManager, ExamineSearchConfiguration configuration,
            ILogger<IndexCommandHandler> logger)
        {
            _examineManager = examineManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task HandleAsync(IndexCommand command)
        {
            var indexName = _configuration.IndexPrefixName + command.Index.ToLowerInvariant();
            _examineManager.TryGetIndex(indexName,
                out IIndex examineIndex);
            if (examineIndex == null)
            {
                _logger.LogError($"Examine index not found {indexName}");
                return;
            }

            try
            {
                if (!examineIndex.IndexExists())
                {
                    examineIndex.CreateIndex();
                }

                examineIndex.IndexItems(TranslateModel(command.Models));
            }
            catch (Exception e)
            {
                _logger.LogError($"Indexing for {indexName} failed", e);
            }
        }

        private IEnumerable<ValueSet> TranslateModel(List<ISearchModel> commandModels)
        {
            var modelList = new List<ValueSet>();
            foreach (var model in commandModels)
            {
                var translatedModel = ValueSet.FromObject(model.Id, "search", model.ContentType, model);

                modelList.Add(translatedModel);
            }

            return modelList;
        }
    }
}