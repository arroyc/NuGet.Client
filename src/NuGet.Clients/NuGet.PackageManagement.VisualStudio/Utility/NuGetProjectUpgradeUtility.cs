// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using NuGet.ProjectManagement;
using NuGet.VisualStudio;

namespace NuGet.PackageManagement.VisualStudio
{
    public class NuGetProjectUpgradeUtility
    {
        private static readonly HashSet<string> UpgradeableProjectTypes =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                VsProjectTypes.CsharpProjectTypeGuid,
                VsProjectTypes.VbProjectTypeGuid,
                VsProjectTypes.FsharpProjectTypeGuid
            };

        private static readonly HashSet<string> UnupgradeableProjectTypes =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                VsProjectTypes.CppProjectTypeGuid,
                VsProjectTypes.WebApplicationProjectTypeGuid,
                VsProjectTypes.WebSiteProjectTypeGuid
            };

        public static async Task<bool> IsNuGetProjectUpgradeableAsync(NuGetProject nuGetProject, Project envDTEProject = null)
        {
            await NuGetUIThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (nuGetProject == null && envDTEProject == null)
            {
                return false;
            }

            if (nuGetProject == null)
            {
                var solutionManager = ServiceLocator.GetInstance<IVsSolutionManager>();

                var projectSafeName = await EnvDTEProjectInfoUtility.GetCustomUniqueNameAsync(envDTEProject);
                nuGetProject = await solutionManager.GetNuGetProjectAsync(projectSafeName);

                if (nuGetProject == null)
                {
                    return false;
                }
            }

            if (!nuGetProject.ProjectServices.Capabilities.SupportsPackageReferences)
            {
                return false;
            }

            var msBuildNuGetProject = nuGetProject as MSBuildNuGetProject;
            if (msBuildNuGetProject == null || !msBuildNuGetProject.PackagesConfigNuGetProject.PackagesConfigExists())
            {
                return false;
            }

            if (envDTEProject == null)
            {
                var vsmsBuildNuGetProjectSystem =
                    msBuildNuGetProject.ProjectSystem as VsMSBuildProjectSystem;
                if (vsmsBuildNuGetProjectSystem == null)
                {
                    return false;
                }
                envDTEProject = vsmsBuildNuGetProjectSystem.VsProjectAdapter.Project;
            }

            if (!EnvDTEProjectUtility.IsSupported(envDTEProject))
            {
                return false;
            }
            var projectGuids = VsHierarchyUtility.GetProjectTypeGuids(envDTEProject);

            if (projectGuids.Any(t => UnupgradeableProjectTypes.Contains(t)))
            {
                return false;
            }

            // Project is supported language, and not an unsupported type
            return UpgradeableProjectTypes.Contains(envDTEProject.Kind) &&
                   projectGuids.All(projectTypeGuid => !SupportedProjectTypes.IsUnsupported(projectTypeGuid));
        }
    }
}
