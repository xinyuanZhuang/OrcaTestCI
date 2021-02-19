using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Orca.Entities;
using Orca.Tools;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Orca.Services;

namespace Orca.Scheduling
{
    /// <summary>
    /// A scheduler used for controlling checking of updated courses on Sharepoint through a background task
    /// </summary>
    public class CourseCatalogUpdater : BackgroundService
    {
        private const int _dELAY_TIME_MS = 5 * 60 * 1000;
        private readonly ILogger<CourseCatalogUpdater> _logger;
        private SharepointCourseCatalog _catalog;

        public CourseCatalogUpdater(ILogger<CourseCatalogUpdater> logger, SharepointCourseCatalog catalog)
        {
            _logger = logger;
            _catalog = catalog;
        }
        // Executes the scheduler in the background. It uses a cancellation token to stop the scheduler at a fixed time period
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation($"CourseCatalogUpdater is starting.");

            stoppingToken.Register(() =>
                _logger.LogInformation($" CourseCatalogUpdater background task is stopping."));

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Updating Course Catalog.");

                await UpdateCourseCatalog();

                await Task.Delay(_dELAY_TIME_MS, stoppingToken);
            }

            _logger.LogInformation($"CourseCatalogUpdater background task is stopping.");
        }

        private async Task UpdateCourseCatalog()
        {
            await _catalog.UpdateInMemoryMapping();
        }

    }
}
