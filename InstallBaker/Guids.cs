// Guids.cs
// MUST match guids.h
using System;

namespace AshokGelal.InstallBaker
{
    static class GuidList
    {
        public const string guidInstallBakerPkgString = "6c51d14e-cdbe-4cc1-a158-3fa4376d51fb";
        public const string guidInstallBakerCmdSetString = "a6ccf1a8-f121-423a-a67e-6af731b72fda";
        public const string guidToolWindowPersistanceString = "e5116d85-19f6-4a60-8271-a6c83f73e49d";

        public static readonly Guid guidInstallBakerCmdSet = new Guid(guidInstallBakerCmdSetString);
    };
}