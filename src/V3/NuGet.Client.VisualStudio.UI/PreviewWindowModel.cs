﻿using System.Collections.Generic;
using System.Linq;
using NuGet.Client.Resolution;

namespace NuGet.Client.VisualStudio.UI
{
    public enum PackagePreviewStatus
    {
        Unchanged,
        Deleted,
        Added
    }

    public class PreviewWindowModel
    {
        private List<PreviewResult> _previewResults;

        public IEnumerable<PreviewResult> PreviewResults
        {
            get
            {
                return _previewResults;
            }
        }

        public PreviewWindowModel(IEnumerable<PackageAction> actions)
        {
            _previewResults = new List<PreviewResult>();
            var projects = actions.Select(a => a.Target)
                .Where(p => p != null)
                .Distinct();
            foreach (var targetProject in projects)
            {
                var packageStatus = targetProject.InstalledPackages.GetInstalledPackages()
                    .Select(p => p.Identity)
                    .ToDictionary(p => p, _ => PackagePreviewStatus.Unchanged);

                foreach (var action in actions.Where(a => targetProject.Equals(a.Target)))
                {
                    if (action.ActionType == PackageActionType.Install)
                    {
                        packageStatus[action.PackageIdentity] = PackagePreviewStatus.Added;
                    }
                    else if (action.ActionType == PackageActionType.Uninstall)
                    {
                        packageStatus[action.PackageIdentity] = PackagePreviewStatus.Deleted;
                    }
                }

                var unchanged = packageStatus
                    .Where(v => v.Value == PackagePreviewStatus.Unchanged)
                    .Select(v => v.Key);
                var deleted = packageStatus
                    .Where(v => v.Value == PackagePreviewStatus.Deleted)
                    .Select(v => v.Key);
                var added = packageStatus
                    .Where(v => v.Value == PackagePreviewStatus.Added)
                    .Select(v => v.Key);
                if (deleted.Any() || added.Any())
                {
                    _previewResults.Add(new PreviewResult(
                        targetProject.Name,
                        unchanged: unchanged,
                        deleted: deleted,
                        added: added));
                }
            }
        }
    }

    public class PreviewResult
    {
        public IEnumerable<PackageIdentity> Deleted
        {
            get;
            private set;
        }

        public IEnumerable<PackageIdentity> Added
        {
            get;
            private set;
        }

        public IEnumerable<PackageIdentity> Unchanged
        {
            get;
            private set;
        }

        public string Name
        {
            get;
            private set;
        }

        public PreviewResult(
            string name,
            IEnumerable<PackageIdentity> added,
            IEnumerable<PackageIdentity> deleted,
            IEnumerable<PackageIdentity> unchanged)
        {
            Name = name;
            Added = added;
            Deleted = deleted;
            Unchanged = unchanged;
        }
    }
}