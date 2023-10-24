// Decompiled with JetBrains decompiler
// Type: WiscMano.Domain.Model.ISnapshot
// Assembly: WiscMano.Domain, Version=1.2.0.0, Culture=neutral, PublicKeyToken=null
// MVID: CFF60A97-1D2E-4EF1-A93D-347D2B8F1172
// Assembly location: C:\Users\USER\AppData\Local\Programs\29 Labs\WiscMano\WiscMano.Domain.dll

using NodaTime;
using SRHWiscMano.Core.Helpers;

namespace SRHWiscMano.Core.Models
{
    public interface ISnapshot
    {
        string Id { get; }

        IExamData Data { get; }

        string Text { get; }

        Instant Time { get; }

        Range<int> SensorRange { get; }

        int? VPUpperBound { get; }

        int? UesLowerBound { get; }

        bool IsSelected { get; }

        bool NormalEligible { get; }

        // ISnapshotLabels Labels { get; }

        IReadOnlyList<IRegion> Regions { get; }

        // RegionsVersion RegionsVersion { get; }
    }

    
}