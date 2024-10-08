﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using RPPP_WebApp.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RPPP_WebApp.Models;

public partial class Plant
{
    public int IdPlant { get; set; }

    public string Species { get; set; }

    public string SpeciesGroup { get; set; }

    public string FruitVegetable { get; set; }

    public string Origin { get; set; }

    public int Quantity { get; set; }

    [ExcelFormat("dd.mm.yyyy")]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [ExcelFormat("dd.mm.yyyy")]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    public string Product { get; set; }

    [ExcelFormat("dd.mm.yyyy")]
    [DisplayFormat(DataFormatString = "{0:dd.MM.yyyy.}", ApplyFormatInEditMode = false)]
    [DataType(DataType.Date)]
    public DateTime ProductDate { get; set; }

    public int IdPlot { get; set; }

    public virtual ICollection<Harvest> Harvests { get; set; } = new List<Harvest>();

    public virtual Plot IdPlotNavigation { get; set; }
}