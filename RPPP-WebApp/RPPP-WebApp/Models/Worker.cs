﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using RPPP_WebApp.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RPPP_WebApp.Models;

public partial class Worker
{
 
    public int IdPerson { get; set; }

    public int? Salary { get; set; }

    public int? Time { get; set; }

    public int? IdHarvest { get; set; }

    public virtual Harvest IdHarvestNavigation { get; set; }
    
    public virtual Person IdPersonNavigation { get ; set; }
}       