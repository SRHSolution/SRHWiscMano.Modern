﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SRHWiscMano.Core.Models;

namespace SRHWiscMano.Core.Services
{
    public interface IExamMetaData
    {
        IPatient PatientData { get; }

    }
}
