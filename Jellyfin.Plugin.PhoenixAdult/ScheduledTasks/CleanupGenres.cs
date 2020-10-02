using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;

namespace PhoenixAdult.ScheduledTasks
{
    public class CleanupGenres : IScheduledTask
    {
        public string Name => "Cleanup Genres";

        public string Key => "PhoenixAdultCleanupGenres";

        public string Description => "Cleanup genres in library";

        public string Category => "PhoenixAdult";

        private readonly ILibraryManager _libraryManager;

        public CleanupGenres(ILibraryManager libraryManager)
        {
            _libraryManager = libraryManager;
        }

        public async Task Execute(CancellationToken cancellationToken, IProgress<double> progress)
        {
            await Task.Yield();
            progress?.Report(0);

            var items = _libraryManager.GetItemList(new InternalItemsQuery
            {
                IsMovie = true
            }).Where(o => o.ProviderIds.ContainsKey(Plugin.Instance.Name)).ToList();

            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var parent = item;

                if (parent.Genres != null && parent.Genres.Any())
                {
                    parent.Genres = Helpers.Genres.Cleanup(parent.Genres, parent.Name);
                    _libraryManager.UpdateItem(item, parent, ItemUpdateType.MetadataEdit, cancellationToken);
                }

                progress?.Report((double)i / items.Count * 100);

                if (cancellationToken.IsCancellationRequested)
                    return;
            }

            progress?.Report(100);
        }

        public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
        {
            yield return new TaskTriggerInfo { Type = TaskTriggerInfo.TriggerWeekly, DayOfWeek = DayOfWeek.Sunday, TimeOfDayTicks = TimeSpan.FromHours(12).Ticks};
        }
    }
}