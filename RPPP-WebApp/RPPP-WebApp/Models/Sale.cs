﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using System.Numerics;

namespace RPPP_WebApp.Models;

public partial class Sale
{
    public Sale()
    {
        Harvests = new HashSet<Harvest>();
    }
    public int IdSale { get; set; }

    public string PlantSeedling { get; set; }

    public int? QuantitySale { get; set; }

    public int? PriceSale { get; set; }

    public int? IdHarvest { get; set; }

    public int? IdPerson { get; set; }

    public int? IdAnonymous { get; set; }

    public virtual ICollection<Harvest> Harvests { get; set; } = new List<Harvest>();

    public virtual Anonymou IdAnonymousNavigation { get; set; }

    public virtual Harvest IdHarvestNavigation { get; set; }

    public virtual KnownCustomer IdPersonNavigation { get; set; }
}