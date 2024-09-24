using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PdfRpt.ColumnsItemsTemplates;
using PdfRpt.Core.Contracts;
using PdfRpt.Core.Helper;
using PdfRpt.FluentInterface;
using Microsoft.Data.SqlClient;
using OfficeOpenXml;
using RPPP_WebApp.Extensions;
using RPPP_WebApp.Models;
using RPPP_WebApp.ModelsPartial;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using static iTextSharp.text.pdf.AcroFields;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;

namespace RPPP_WebApp.Controllers
{
    public class ReportController : Controller

    {
        private readonly RPPP13Context ctx;
        private readonly IWebHostEnvironment environment;
        private const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ReportController(RPPP13Context ctx, IWebHostEnvironment environment)
        {
            this.ctx = ctx;
            this.environment = environment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> PlotExcel()
        {
            var plot = await ctx.Plots
                           .AsNoTracking()
                           .OrderBy(d => d.PlotName)
                           .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Plot list";
                excel.Workbook.Properties.Author = "Fofana RPPP13";
                var worksheet = excel.Workbook.Worksheets.Add("Plots");


                worksheet.Cells[1, 1].Value = "Plot Id";
                worksheet.Cells[1, 2].Value = "Plot Name";
                worksheet.Cells[1, 3].Value = "Plot GPS";
                worksheet.Cells[1, 4].Value = "Infrastructure";
                worksheet.Cells[1, 5].Value = "Material";
                worksheet.Cells[1, 6].Value = "Sun Intensity";
                worksheet.Cells[1, 7].Value = "Id Lease";
                worksheet.Cells[1, 8].Value = "Id Soil";

                for (int i = 0; i < plot.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = plot[i].IdPlot;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = plot[i].PlotName;
                    worksheet.Cells[i + 2, 3].Value = plot[i].PlotGps;
                    worksheet.Cells[i + 2, 4].Value = plot[i].Infrastructure;
                    worksheet.Cells[i + 2, 5].Value = plot[i].Material;
                    worksheet.Cells[i + 2, 6].Value = plot[i].SunIntensity;
                    worksheet.Cells[i + 2, 7].Value = plot[i].IdLease;
                    worksheet.Cells[i + 2, 8].Value = plot[i].IdSoil;
                }
                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "Plots.xlsx");
            
        }

        public async Task<IActionResult> PlotExcelMD()
        {
            var plot = await ctx.Plots 
                            .Include(d => d.Plants)
                           .AsNoTracking()
                           .OrderBy(d => d.IdPlot)
                           .ToListAsync();



            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Plot list";
                excel.Workbook.Properties.Author = "Fofana RPPP13";

                for (int i = 0; i < plot.Count; i++)
                {

                    var plotItem = plot[i];
                    var worksheet = excel.Workbook.Worksheets.Add(plotItem.PlotName);


                    worksheet.Cells[1, 1].Value = "Plot Id";
                    worksheet.Cells[1, 2].Value = "Plot Name";
                    worksheet.Cells[1, 3].Value = "Plot GPS";
                    worksheet.Cells[1, 4].Value = "Infrastructure";
                    worksheet.Cells[1, 5].Value = "Material";
                    worksheet.Cells[1, 6].Value = "Sun Intensity";
                    worksheet.Cells[1, 7].Value = "Id Lease";
                    worksheet.Cells[1, 8].Value = "Id Soil";



                    worksheet.Cells[2, 1].Value = plot[i].IdPlot;
                    worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2, 2].Value = plot[i].PlotName;
                    worksheet.Cells[2, 3].Value = plot[i].PlotGps;
                    worksheet.Cells[2, 4].Value = plot[i].Infrastructure;
                    worksheet.Cells[2, 5].Value = plot[i].Material;
                    worksheet.Cells[2, 6].Value = plot[i].SunIntensity;
                    worksheet.Cells[2, 7].Value = plot[i].IdLease;
                    worksheet.Cells[2, 8].Value = plot[i].IdSoil;


                    var plantList = plotItem.Plants.Where(d => d.IdPlot == plotItem.IdPlot).ToList();

                    worksheet.Cells[3, 1].Value = "Plant Id";
                    worksheet.Cells[3, 2].Value = "Species";
                    worksheet.Cells[3, 3].Value = "Species Group";
                    worksheet.Cells[3, 4].Value = "Fruit/Vegetable";
                    worksheet.Cells[3, 5].Value = "Origin";
                    worksheet.Cells[3, 6].Value = "Quantity";
                    worksheet.Cells[3, 7].Value = "Start Date";
                    worksheet.Cells[3, 8].Value = "End Date";
                    worksheet.Cells[3, 9].Value = "Product";
                    worksheet.Cells[3, 10].Value = "Product Date";

                    int numbercell = 4;
                    foreach (var plant in plantList)
                    {
                        worksheet.Cells[numbercell, 1].Value = plant.IdPlant;
                        worksheet.Cells[numbercell, 2].Value = plant.Species;
                        worksheet.Cells[numbercell, 3].Value = plant.SpeciesGroup;
                        worksheet.Cells[numbercell, 4].Value = plant.FruitVegetable;
                        worksheet.Cells[numbercell, 5].Value = plant.Origin;
                        worksheet.Cells[numbercell, 6].Value = plant.Quantity;
                        worksheet.Cells[numbercell, 7].Value = plant.StartDate;
                        worksheet.Cells[numbercell, 8].Value = plant.EndDate;
                        worksheet.Cells[numbercell, 9].Value = plant.Product;
                        worksheet.Cells[numbercell, 10].Value = plant.ProductDate;
                        numbercell++;
                    }
                }



                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "PlotsMD.xlsx");

        }

        public async Task<IActionResult> HarvestExcel()
        {
            var harvest = await ctx.Harvests
                           .AsNoTracking()
                           .OrderBy(d => d.IdHarvest)
                           .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Harvest list";
                excel.Workbook.Properties.Author = "VARATHARAJAN RPPP13";
                var worksheet = excel.Workbook.Worksheets.Add("Harvests");


                worksheet.Cells[1, 1].Value = "Harvest ID";
                worksheet.Cells[1, 2].Value = "Harvest Name";
                worksheet.Cells[1, 3].Value = "Harvest Cost ";
                worksheet.Cells[1, 4].Value = "Start Harvest";
                worksheet.Cells[1, 5].Value = "End Harvest";
                worksheet.Cells[1, 6].Value = " Quantity";
                worksheet.Cells[1, 7].Value = "Id Plant";
                worksheet.Cells[1, 8].Value = "Id Sale";
                worksheet.Cells[1, 9].Value = "Use Harvest";



                for (int i = 0; i < harvest.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = harvest[i].IdHarvest;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = harvest[i].NameHarvest;
                    worksheet.Cells[i + 2, 3].Value = harvest[i].CostHarvest;
                    worksheet.Cells[i + 2, 4].Value = harvest[i].StartHarvest;
                    worksheet.Cells[i + 2, 5].Value = harvest[i].EndHarvest;
                    worksheet.Cells[i + 2, 6].Value = harvest[i].QuantityHarvest;
                    worksheet.Cells[i + 2, 7].Value = harvest[i].IdPlant;
                    worksheet.Cells[i + 2, 8].Value = harvest[i].IdSale;
                    worksheet.Cells[i + 2, 9].Value = harvest[i].UseHarvest;

                }
                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "Harvest.xlsx");

        }
        public async Task<IActionResult> HarvestExcelMD()
        {
            var harvest = await ctx.Harvests
                            .Include(d => d.Workers)
                            .ThenInclude(z=>z.IdPersonNavigation)
                           .AsNoTracking()
                           .OrderBy(d => d.IdHarvest)
                           .ToListAsync();



            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Harvest list";
                excel.Workbook.Properties.Author = "VARATHARAJAN RPPP13";

                for (int i = 0; i < harvest.Count; i++)
                {

                    var harvestItem = harvest[i];
                    var worksheet = excel.Workbook.Worksheets.Add(harvestItem.NameHarvest);


                    worksheet.Cells[1, 1].Value = "Harvest ID";
                    worksheet.Cells[1, 2].Value = "Harvest Name";
                    worksheet.Cells[1, 3].Value = "Harvest Cost ";
                    worksheet.Cells[1, 4].Value = "Start Harvest";
                    worksheet.Cells[1, 5].Value = "End Harvest";
                    worksheet.Cells[1, 6].Value = " Quantity";
                    worksheet.Cells[1, 7].Value = "Id Plant";
                    worksheet.Cells[1, 8].Value = "Id Sale";
                    worksheet.Cells[1, 9].Value = "Use Harvest";

                    worksheet.Cells[2, 1].Value = harvest[i].IdHarvest;
                    worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2, 2].Value = harvest[i].NameHarvest;
                    worksheet.Cells[2, 3].Value = harvest[i].CostHarvest;
                    worksheet.Cells[2, 4].Value = harvest[i].StartHarvest;
                    worksheet.Cells[2, 5].Value = harvest[i].EndHarvest;
                    worksheet.Cells[2, 6].Value = harvest[i].QuantityHarvest;
                    worksheet.Cells[2, 7].Value = harvest[i].IdPlant;
                    worksheet.Cells[2, 8].Value = harvest[i].IdSale;
                    worksheet.Cells[2, 9].Value = harvest[i].UseHarvest;



                    var workerList = harvestItem.Workers.Where(d => d.IdHarvest == harvestItem.IdHarvest).ToList();

                    worksheet.Cells[3, 1].Value = "Worker ID";
                    worksheet.Cells[3, 2].Value = "First Name";
                    worksheet.Cells[3, 3].Value = "Last Name";
                    worksheet.Cells[3, 4].Value = "Address";
                    worksheet.Cells[3, 5].Value = "Number";
                    worksheet.Cells[3, 6].Value = "Mail";
                    worksheet.Cells[3, 7].Value = "Time";
                    worksheet.Cells[3, 8].Value = "Salary";


                    int numbercell = 4;

                    foreach (var worker in workerList)
                    {
                        worksheet.Cells[numbercell, 1].Value = worker.IdPerson;
                        worksheet.Cells[numbercell, 2].Value = worker.IdPersonNavigation.FirstName;
                        worksheet.Cells[numbercell, 3].Value = worker.IdPersonNavigation.LastName;
                        worksheet.Cells[numbercell, 4].Value = worker.IdPersonNavigation.Address;
                        worksheet.Cells[numbercell, 5].Value = worker.IdPersonNavigation.Number;
                        worksheet.Cells[numbercell, 6].Value = worker.IdPersonNavigation.Mail;
                        worksheet.Cells[numbercell, 7].Value = worker.Time;
                        worksheet.Cells[numbercell, 8].Value = worker.Salary;

                        numbercell++;
                    }
                }



                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "HarvestsMD.xlsx");

        }
        public async Task<IActionResult> PlantExcel()
        {
            var plant = await ctx.Plants
                           .AsNoTracking()
                           .OrderBy(d => d.IdPlant)
                           .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Plant list";
                excel.Workbook.Properties.Author = "Saad Chaouch RPPP13";
                var worksheet = excel.Workbook.Worksheets.Add("Plants");


                worksheet.Cells[1, 1].Value = "Plant Id";
                worksheet.Cells[1, 2].Value = "Species";
                worksheet.Cells[1, 3].Value = "Species Group";
                worksheet.Cells[1, 4].Value = "Fruit or Vegetable";
                worksheet.Cells[1, 5].Value = "Origin";
                worksheet.Cells[1, 6].Value = "Quantity";
                worksheet.Cells[1, 7].Value = "Product";

                for (int i = 0; i < plant.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = plant[i].IdPlant;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = plant[i].Species;
                    worksheet.Cells[i + 2, 3].Value = plant[i].SpeciesGroup;
                    worksheet.Cells[i + 2, 4].Value = plant[i].FruitVegetable;
                    worksheet.Cells[i + 2, 5].Value = plant[i].Origin;
                    worksheet.Cells[i + 2, 6].Value = plant[i].Quantity;
                    worksheet.Cells[i + 2, 7].Value = plant[i].Product;
                }
                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "Plants.xlsx");

        }

        public async Task<IActionResult> PlantExcelMD()
        {
            var plant = await ctx.Plants
                            .Include(d => d.Harvests)
                           .AsNoTracking()
                           .OrderBy(d => d.IdPlant)
                           .ToListAsync();



            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Plant list";
                excel.Workbook.Properties.Author = "Saad saouch RPPP13";

                for (int i = 0; i < plant.Count; i++)
                {

                    var plantItem = plant[i];
                    var worksheet = excel.Workbook.Worksheets.Add(plantItem.Species);


                    worksheet.Cells[1, 1].Value = "Plant Id";
                    worksheet.Cells[1, 2].Value = "Species";
                    worksheet.Cells[1, 3].Value = "Species Group";
                    worksheet.Cells[1, 4].Value = "Fruit/Vegetable";
                    worksheet.Cells[1, 5].Value = "Origin";
                    worksheet.Cells[1, 6].Value = "Quantity";
                    worksheet.Cells[1, 7].Value = "Start Date";
                    worksheet.Cells[1, 8].Value = "End Date";
                    worksheet.Cells[1, 9].Value = "Product";
                    worksheet.Cells[1, 10].Value = "Product Date";



                    worksheet.Cells[2, 1].Value = plant[i].IdPlant;
                    worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2, 2].Value = plant[i].Species;
                    worksheet.Cells[2, 3].Value = plant[i].SpeciesGroup;
                    worksheet.Cells[2, 4].Value = plant[i].FruitVegetable;
                    worksheet.Cells[2, 5].Value = plant[i].Origin;
                    worksheet.Cells[2, 6].Value = plant[i].Quantity;
                    worksheet.Cells[2, 7].Value = plant[i].StartDate;
                    worksheet.Cells[2, 8].Value = plant[i].EndDate;
                    worksheet.Cells[2, 9].Value = plant[i].Product;
                    worksheet.Cells[2, 10].Value = plant[i].ProductDate;


                    var harvestList = plantItem.Harvests.Where(d => d.IdPlant == plantItem.IdPlant).ToList();

                    worksheet.Cells[3, 1].Value = "Harvest Id";
                    worksheet.Cells[3, 2].Value = "Start Harvest";
                    worksheet.Cells[3, 3].Value = "End Harvest";
                    worksheet.Cells[3, 4].Value = "USe Harvest";
                    worksheet.Cells[3, 5].Value = "Quantity Harvest";
                    worksheet.Cells[3, 6].Value = "Cost Harvest";
                    worksheet.Cells[3, 7].Value = "Id plants";
                    worksheet.Cells[3, 8].Value = "Id Sale";
                    worksheet.Cells[3, 9].Value = "Name Harvest";

                    int numbercell = 4;
                    foreach (var harvest in harvestList)
                    {
                        worksheet.Cells[numbercell, 1].Value = harvest.IdHarvest;
                        worksheet.Cells[numbercell, 2].Value = harvest.StartHarvest;
                        worksheet.Cells[numbercell, 3].Value = harvest.EndHarvest;
                        worksheet.Cells[numbercell, 4].Value = harvest.UseHarvest;
                        worksheet.Cells[numbercell, 5].Value = harvest.QuantityHarvest;
                        worksheet.Cells[numbercell, 6].Value = harvest.CostHarvest;
                        worksheet.Cells[numbercell, 7].Value = harvest.IdPlant;
                        worksheet.Cells[numbercell, 8].Value = harvest.IdSale;
                        worksheet.Cells[numbercell, 9].Value = harvest.NameHarvest;
                        numbercell++;
                    }
                }



                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "PlantsMD.xlsx");

        }


        public async Task<IActionResult> SaleExcel()
        {
            var sale = await ctx.Sales
                           .AsNoTracking()
                           .OrderBy(d => d.IdSale)
                           .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Sale list";
                excel.Workbook.Properties.Author = "Andriamalala RPPP13";
                var worksheet = excel.Workbook.Worksheets.Add("Plants");


                worksheet.Cells[1, 1].Value = "Sale Id";
                worksheet.Cells[1, 2].Value = "PlantSeedling";
                worksheet.Cells[1, 3].Value = "QuantitySale";
                worksheet.Cells[1, 4].Value = "PriceSale";
                worksheet.Cells[1, 5].Value = "Id Harvest";
                worksheet.Cells[1, 6].Value = "Id Person";
                worksheet.Cells[1, 7].Value = "Id Anonymous";

                for (int i = 0; i < sale.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = sale[i].IdSale;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = sale[i].PlantSeedling;
                    worksheet.Cells[i + 2, 3].Value = sale[i].QuantitySale;
                    worksheet.Cells[i + 2, 4].Value = sale[i].PriceSale;
                    worksheet.Cells[i + 2, 5].Value = sale[i].IdHarvest;
                    worksheet.Cells[i + 2, 6].Value = sale[i].IdPerson;
                    worksheet.Cells[i + 2, 7].Value = sale[i].IdAnonymous;
                }
                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "Sales.xlsx");

        }

        public async Task<IActionResult> SaleExcelMD()
        {
            var sale = await ctx.Sales
                            .Include(d => d.Harvests)
                           .AsNoTracking()
                           .OrderBy(d => d.IdSale)
                           .ToListAsync();



            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Sale list";
                excel.Workbook.Properties.Author = "Andriamalala RPPP13";

                for (int i = 0; i < sale.Count; i++)
                {

                    var saleItem = sale[i];
                    var worksheet = excel.Workbook.Worksheets.Add(saleItem.PlantSeedling);


                    worksheet.Cells[1, 1].Value = "Sale Id";
                    worksheet.Cells[1, 2].Value = "PlantSeedling";
                    worksheet.Cells[1, 3].Value = "Quantity Sale";
                    worksheet.Cells[1, 4].Value = "Price Sale";
                    worksheet.Cells[1, 5].Value = "Id Harvest";
                    worksheet.Cells[1, 6].Value = "Id Person";
                    worksheet.Cells[1, 7].Value = "Id Anonymous";



                    worksheet.Cells[2, 1].Value = sale[i].IdSale;
                    worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[2, 2].Value = sale[i].PlantSeedling;
                    worksheet.Cells[2, 3].Value = sale[i].QuantitySale;
                    worksheet.Cells[2, 4].Value = sale[i].PriceSale;
                    worksheet.Cells[2, 5].Value = sale[i].IdHarvest;
                    worksheet.Cells[2, 6].Value = sale[i].IdPerson;
                    worksheet.Cells[2, 7].Value = sale[i].IdAnonymous;


                    var harvestList = saleItem.Harvests.Where(d => d.IdSale == saleItem.IdSale).ToList();

                    worksheet.Cells[3, 1].Value = "Harvest Id";
                    worksheet.Cells[3, 2].Value = "Start Harvest";
                    worksheet.Cells[3, 3].Value = "End Harvest";
                    worksheet.Cells[3, 4].Value = "USe Harvest";
                    worksheet.Cells[3, 5].Value = "Quantity Harvest";
                    worksheet.Cells[3, 6].Value = "Cost Harvest";
                    worksheet.Cells[3, 7].Value = "Id plants";
                    worksheet.Cells[3, 8].Value = "Id Sale";
                    worksheet.Cells[3, 9].Value = "Name Harvest";

                    int numbercell = 4;
                    foreach (var harvest in harvestList)
                    {
                        worksheet.Cells[numbercell, 1].Value = harvest.IdHarvest;
                        worksheet.Cells[numbercell, 2].Value = harvest.StartHarvest;
                        worksheet.Cells[numbercell, 3].Value = harvest.EndHarvest;
                        worksheet.Cells[numbercell, 4].Value = harvest.UseHarvest;
                        worksheet.Cells[numbercell, 5].Value = harvest.QuantityHarvest;
                        worksheet.Cells[numbercell, 6].Value = harvest.CostHarvest;
                        worksheet.Cells[numbercell, 7].Value = harvest.IdPlant;
                        worksheet.Cells[numbercell, 8].Value = harvest.IdSale;
                        worksheet.Cells[numbercell, 9].Value = harvest.NameHarvest;
                        numbercell++;
                    }
                }



                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "SalesMD.xlsx");

        }

        public async Task<IActionResult> RequestExcel()
        {
            var request = await ctx.Requests
                           .AsNoTracking()
                           .OrderBy(d => d.IdRequest)
                           .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Request list";
                excel.Workbook.Properties.Author = "Andriamalala RPPP13";
                var worksheet = excel.Workbook.Worksheets.Add("Requests");


                worksheet.Cells[1, 1].Value = "Request Id";
                worksheet.Cells[1, 2].Value = "Species Asked";
                worksheet.Cells[1, 3].Value = "Date Request";
                worksheet.Cells[1, 4].Value = "Quantity Asked";
                worksheet.Cells[1, 5].Value = "Price Asked";
                worksheet.Cells[1, 6].Value = "Id Person";
                worksheet.Cells[1, 7].Value = "Status Request";

                for (int i = 0; i < request.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = request[i].IdRequest;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = request[i].SpeciesAsked;
                    worksheet.Cells[i + 2, 3].Value = request[i].DateRequest;
                    worksheet.Cells[i + 2, 4].Value = request[i].QuantityAsked;
                    worksheet.Cells[i + 2, 5].Value = request[i].PriceAsked;
                    worksheet.Cells[i + 2, 6].Value = request[i].IdPerson;
                    worksheet.Cells[i + 2, 7].Value = request[i].StatusRequest;
                }
                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "Requests.xlsx");

        }

       /* public async Task<IActionResult> RequestExcelMD()
        {
             var request = await ctx.Requests
                            .Include(d => d.KnownCustomers)
                            .AsNoTracking()
                            .OrderBy(d => d.IdRequest)
                            .ToListAsync();



             byte[] content;
             using (ExcelPackage excel = new ExcelPackage())
             {
                 excel.Workbook.Properties.Title = "Request list";
                 excel.Workbook.Properties.Author = "Andriamalala RPPP13";

                 for (int i = 0; i < request.Count; i++)
                 {

                     var requestItem = request[i];
                     var worksheet = excel.Workbook.Worksheets.Add(requestItem.StatusRequest);


                     worksheet.Cells[1, 1].Value = "Request Id";
                     worksheet.Cells[1, 2].Value = "Species Asked";
                     worksheet.Cells[1, 3].Value = "Date Request";
                     worksheet.Cells[1, 4].Value = "QuantityAsked";
                     worksheet.Cells[1, 5].Value = "Price Asked";
                     worksheet.Cells[1, 6].Value = "Id Person";
                     worksheet.Cells[1, 7].Value = "StatusRequest";



                     worksheet.Cells[2, 1].Value = request[i].IdRequest;
                     worksheet.Cells[2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                     worksheet.Cells[2, 2].Value = request[i].PlantSeedling;
                     worksheet.Cells[2, 3].Value = request[i].QuantitySale;
                     worksheet.Cells[2, 4].Value = request[i].PriceSale;
                     worksheet.Cells[2, 5].Value = request[i].IdHarvest;
                     worksheet.Cells[2, 6].Value = request[i].IdPerson;
                     worksheet.Cells[2, 7].Value = request[i].IdAnonymous;


                     var personList = requestItem.KnownCustomers.Where(d => d.IdRequest == requestItem.IdRequest).ToList();

                     worksheet.Cells[3, 1].Value = "Person Id";
                     worksheet.Cells[3, 2].Value = "First Name";
                     worksheet.Cells[3, 3].Value = "Last Name";
                     worksheet.Cells[3, 4].Value = "Number";
                     worksheet.Cells[3, 5].Value = "Mail";
                     worksheet.Cells[3, 6].Value = "Adresse";

                     int numbercell = 4;
                     foreach (var person in personList)
                     {
                         worksheet.Cells[numbercell, 1].Value = person.IdPerson;
                         worksheet.Cells[numbercell, 2].Value = person.FirstName;
                         worksheet.Cells[numbercell, 3].Value = person.LastName;
                         worksheet.Cells[numbercell, 4].Value = person.Number;
                         worksheet.Cells[numbercell, 5].Value = person.Mail;
                         worksheet.Cells[numbercell, 6].Value = person.Adresse;
                         numbercell++;
                     }
                 }



                 content = excel.GetAsByteArray();
             }

             return File(content, ExcelContentType, "RequestsMD.xlsx");

        } */

        private PdfReport CreateReport(string title)
        {
            var pdf = new PdfReport();
            pdf.DocumentPreferences(doc => {
                doc.Orientation(PageOrientation.Portrait);
                doc.PageSize(PdfPageSize.A4);
                doc.DocumentMetadata(new DocumentMetadata
                {
                    Author = "Group 13",
                    Application = "RPP13WebApp.MVC",
                    Title = title
                });
                doc.Compression(new CompressionSettings
                {
                    EnableCompression = true,
                    EnableFullCompression = true
                });
            })
                .DefaultFonts(fonts => {
                    fonts.Path(Path.Combine(environment.WebRootPath, "fonts", "Verdana.ttf"),
                                     Path.Combine(environment.WebRootPath, "fonts", "Tahoma.ttf"));
                    fonts.Size(9);
                    fonts.Color(System.Drawing.Color.Black);
                })

        .MainTableTemplate(template =>
        {
            template.BasicTemplate(BasicTemplate.ProfessionalTemplate);
        })
        .MainTablePreferences(table =>
        {
            table.ColumnsWidthsType(TableColumnWidthType.Relative);
            //table.NumberOfDataRowsPerPage(20);
            table.GroupsPreferences(new GroupsPreferences
            {
                GroupType = GroupType.HideGroupingColumns,
                RepeatHeaderRowPerGroup = true,
                ShowOneGroupPerPage = true,
                SpacingBeforeAllGroupsSummary = 5f,
                NewGroupAvailableSpacingThreshold = 150,
                SpacingAfterAllGroupsSummary = 5f
            });
            table.SpacingAfter(4f);
        });

            return pdf;
        }

        public async Task<IActionResult> Plotonly()
        {
            string title = "Plot";
            var plot = await ctx.Plots
                                .AsNoTracking()
                                .OrderBy(d => d.IdPlot)
                                .ToListAsync();
            PdfReport report = CreateReport("Plot");
            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM,yyyy")); })
                    .PagesHeader(header => { header.CacheHeader(cache: true);
                        header.DefaultHeader(defaultHeader => { defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                            defaultHeader.Message(title);
                        });
                    });
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(plot));
            report.MainTableColumns(columns => {
                columns.AddColumn(column => {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true); column.Order(0); column.Width(1);
                    column.HeaderCell("#",
                    horizontalAlignment: HorizontalAlignment.Right);
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plot>(x => x.IdPlot);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1); column.Width(2);
                    column.HeaderCell("Plot ID");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plot>(x => x.PlotName);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2); column.Width(2);
                    column.HeaderCell("Plot Name");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plot>(x => x.PlotGps);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3); column.Width(3);
                    column.HeaderCell("Plot GPS");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plot>(x => x.Infrastructure);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4); column.Width(4);
                    column.HeaderCell("Infrastructure");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plot>(x => x.Material);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5); column.Width(4);
                    column.HeaderCell("Material");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plot>(x => x.SunIntensity);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(6); column.Width(2);
                    column.HeaderCell("Sun Intensity");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plot>(x => x.IdLease);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(7); column.Width(2);
                    column.HeaderCell("Id Lease");
                });

                columns.AddColumn(column => {
                    column.PropertyName<Plot>(x => x.IdSoil);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(8); column.Width(2);
                    column.HeaderCell("Id soil");
                });


            });

            byte[] pdf = report.GenerateAsByteArray();
            if (pdf != null)
            {
                Response.Headers.Add("content-disposition",
                "inline; filename=plot.pdf");
                return File(pdf, "application/pdf");
                //return File(pdf, "application/pdf", "plot.pdf");
            }
            else return NotFound();
        }


        public async Task<IActionResult> Harvestonly()
        {
            string title = "Harvest";
            var harvests = await ctx.Harvests
                                .AsNoTracking()
                                .OrderBy(d => d.IdHarvest)
                                .ToListAsync();
            PdfReport report = CreateReport("Harvest");
            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM,yyyy")); })
                    .PagesHeader(header => {
                        header.CacheHeader(cache: true);
                        header.DefaultHeader(defaultHeader => {
                            defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                            defaultHeader.Message(title);
                        });
                    });
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(harvests));
            report.MainTableColumns(columns => {
                columns.AddColumn(column => {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true); column.Order(0); column.Width(1);
                    column.HeaderCell("#",
                    horizontalAlignment: HorizontalAlignment.Right);
                });
                columns.AddColumn(column => {
                    column.PropertyName<Harvest>(x => x.IdHarvest);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(9); column.Width(2);
                    column.HeaderCell("Harvest ID");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Harvest>(x => x.NameHarvest);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1); column.Width(2);
                    column.HeaderCell("Name Harvest");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Harvest>(x => x.CostHarvest);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2); column.Width(2);
                    column.HeaderCell("Cost Harvest");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Harvest>(x => x.StartHarvest);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3); column.Width(3);
                    column.HeaderCell("Start Harvest");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Harvest>(x => x.EndHarvest);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4); column.Width(4);
                    column.HeaderCell("End Harvest");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Harvest>(x => x.QuantityHarvest);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5); column.Width(4);
                    column.HeaderCell("Quantity");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Harvest>(x => x.IdPlant);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(6); column.Width(2);
                    column.HeaderCell("Id Plant");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Harvest>(x => x.IdSale);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(7); column.Width(2);
                    column.HeaderCell("Id Sale");
                });

                columns.AddColumn(column => {
                    column.PropertyName<Harvest>(x => x.UseHarvest);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(8); column.Width(2);
                    column.HeaderCell("Use Harvest");
                });


            });
            byte[] pdf = report.GenerateAsByteArray();
            if (pdf != null)
            {
                Response.Headers.Add("content-disposition",
                "inline; filename=harvest.pdf");
                return File(pdf, "application/pdf");
              
            }
            else return NotFound();
        }










        public async Task<IActionResult> Plantonly()
        {
            string title = "Plant";
            var plant = await ctx.Plants
                                .AsNoTracking()
                                .OrderBy(d => d.IdPlant)
                                .ToListAsync();
            PdfReport report = CreateReport("Plant");
            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM,yyyy")); })
                    .PagesHeader(header => {
                        header.CacheHeader(cache: true);
                        header.DefaultHeader(defaultHeader => {
                            defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                            defaultHeader.Message(title);
                        });
                    });
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(plant));
            report.MainTableColumns(columns => {
                columns.AddColumn(column => {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true); column.Order(0); column.Width(1);
                    column.HeaderCell("#",
                    horizontalAlignment: HorizontalAlignment.Right);
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plant>(x => x.IdPlant);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1); column.Width(2);
                    column.HeaderCell("Plant ID");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plant>(x => x.Species);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2); column.Width(2);
                    column.HeaderCell("Species");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plant>(x => x.SpeciesGroup);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3); column.Width(3);
                    column.HeaderCell("Species Group");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plant>(x => x.FruitVegetable);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4); column.Width(4);
                    column.HeaderCell("Fruit or Vegetable");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plant>(x => x.Origin);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5); column.Width(4);
                    column.HeaderCell("Origin");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plant>(x => x.Quantity);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(6); column.Width(2);
                    column.HeaderCell("Quantity");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Plant>(x => x.Product);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(7); column.Width(2);
                    column.HeaderCell("Product");
                });

            });

            byte[] pdf = report.GenerateAsByteArray();
            if (pdf != null)
            {
                Response.Headers.Add("content-disposition",
                "inline; filename=plant.pdf");
                return File(pdf, "application/pdf");
                //return File(pdf, "application/pdf", "plant.pdf");
            }
            else return NotFound();
        }

        public async Task<IActionResult> Saleonly()
        {
            string title = "Sale";
            var sale = await ctx.Sales
                                .AsNoTracking()
                                .OrderBy(d => d.IdSale)
                                .ToListAsync();
            PdfReport report = CreateReport("Sale");
            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM,yyyy")); })
                    .PagesHeader(header => {
                        header.CacheHeader(cache: true);
                        header.DefaultHeader(defaultHeader => {
                            defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                            defaultHeader.Message(title);
                        });
                    });
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(sale));
            report.MainTableColumns(columns => {
                columns.AddColumn(column => {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true); column.Order(0); column.Width(1);
                    column.HeaderCell("#",
                    horizontalAlignment: HorizontalAlignment.Right);
                });
                columns.AddColumn(column => {
                    column.PropertyName<Sale>(x => x.IdSale);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1); column.Width(2);
                    column.HeaderCell("Sale ID");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Sale>(x => x.PlantSeedling);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2); column.Width(2);
                    column.HeaderCell("Plant Seedling");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Sale>(x => x.QuantitySale);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3); column.Width(3);
                    column.HeaderCell("Quantity Sale");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Sale>(x => x.PriceSale);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4); column.Width(4);
                    column.HeaderCell("Price Sale");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Sale>(x => x.IdHarvest);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5); column.Width(4);
                    column.HeaderCell("Id Harvest");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Sale>(x => x.IdPerson);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(6); column.Width(2);
                    column.HeaderCell("Id Person");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Sale>(x => x.IdAnonymous);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(7); column.Width(2);
                    column.HeaderCell("Id Anonymous");
                });
            });

            byte[] pdf = report.GenerateAsByteArray();
            if (pdf != null)
            {
                Response.Headers.Add("content-disposition",
                "inline; filename=sale.pdf");
                return File(pdf, "application/pdf");
            }
            else return NotFound();
        }



        public async Task<IActionResult> Knowncustomeronly()
        {
            string title = "KnownCustomer";
            var knowncustomer = await ctx.KnownCustomers
                                .Include(d=>d.IdPersonNavigation)
                                .AsNoTracking()
                                .OrderBy(d => d.IdPerson)
                                .ToListAsync();
            PdfReport report = CreateReport("KnownCustomer");
            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM,yyyy")); })
                    .PagesHeader(header => {
                        header.CacheHeader(cache: true);
                        header.DefaultHeader(defaultHeader => {
                            defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                            defaultHeader.Message(title);
                        });
                    });
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(knowncustomer));
            report.MainTableColumns(columns => {
                columns.AddColumn(column => {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true); column.Order(0); column.Width(1);
                    column.HeaderCell("#",
                    horizontalAlignment: HorizontalAlignment.Right);
                });
                columns.AddColumn(column => {
                    column.PropertyName<KnownCustomer>(x => x.IdPerson);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(9); column.Width(2);
                    column.HeaderCell("Person ID");
                });
                columns.AddColumn(column => {
                    column.PropertyName<KnownCustomer>(x => x.IdPersonNavigation.FirstName);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1); column.Width(2);
                    column.HeaderCell("First Name");
                });
                columns.AddColumn(column => {
                    column.PropertyName<KnownCustomer>(x => x.IdPersonNavigation.LastName);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2); column.Width(2);
                    column.HeaderCell("Last Name");
                });
                columns.AddColumn(column => {
                    column.PropertyName<KnownCustomer>(x => x.IdPersonNavigation.Number);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3); column.Width(3);
                    column.HeaderCell("Number");
                });
                columns.AddColumn(column => {
                    column.PropertyName<KnownCustomer>(x => x.IdPersonNavigation.Address);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4); column.Width(4);
                    column.HeaderCell("Adresse");
                });
                columns.AddColumn(column => {
                    column.PropertyName<KnownCustomer>(x => x.IdPersonNavigation.Mail);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5); column.Width(4);
                    column.HeaderCell("Mail");
                });
                



            });
            byte[] pdf = report.GenerateAsByteArray();
            if (pdf != null)
            {
                Response.Headers.Add("content-disposition",
                "inline; filename=knowncustomer.pdf");
                return File(pdf, "application/pdf");

            }
            else return NotFound();
        }

        public async Task<IActionResult> KnowncustomerExcel()
        {
            var knowncustomer = await ctx.KnownCustomers
                            .Include(d=>d.IdPersonNavigation)
                           .AsNoTracking()
                           .OrderBy(d => d.IdPerson)
                           .ToListAsync();

            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Known Customer list";
                excel.Workbook.Properties.Author = "VARATHARAJAN  & Saad saouch RPPP13";
                var worksheet = excel.Workbook.Worksheets.Add("KnownCustomer");


                worksheet.Cells[1, 1].Value = "Person Id";
                worksheet.Cells[1, 2].Value = "First Name";
                worksheet.Cells[1, 3].Value = "Last Name";
                worksheet.Cells[1, 4].Value = "Number";
                worksheet.Cells[1, 5].Value = "Mail";
                worksheet.Cells[1, 6].Value = "Adresse";
        

                for (int i = 0; i < knowncustomer.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = knowncustomer[i].IdPerson;
                    worksheet.Cells[i + 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[i + 2, 2].Value = knowncustomer[i].IdPersonNavigation.FirstName;
                    worksheet.Cells[i + 2, 3].Value = knowncustomer[i].IdPersonNavigation.LastName;
                    worksheet.Cells[i + 2, 4].Value = knowncustomer[i].IdPersonNavigation.Number;
                    worksheet.Cells[i + 2, 5].Value = knowncustomer[i].IdPersonNavigation.Mail;
                    worksheet.Cells[i + 2, 6].Value = knowncustomer[i].IdPersonNavigation.Address;
                  
                }
                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "KnownCustomer.xlsx");

        }

        public async Task<IActionResult> KnowncustomerExcelMD()
        {
            var knowncustomer = await ctx.KnownCustomers
                            .Include(d => d.IdPersonNavigation)
                           .AsNoTracking()
                           .OrderBy(d => d.IdPerson)
                           .ToListAsync();




            byte[] content;
            using (ExcelPackage excel = new ExcelPackage())
            {
                excel.Workbook.Properties.Title = "Known Customer list";
                excel.Workbook.Properties.Author = "VARATHARAJAN & Saad saouch RPPP13";

                for (int i = 0; i < knowncustomer.Count; i++)
                {

                    var knowcustomerItem = knowncustomer[i];
                    var worksheet = excel.Workbook.Worksheets.Add(knowcustomerItem.IdPersonNavigation.LastName);


                    worksheet.Cells[1, 1].Value = "Person Id";
                    worksheet.Cells[1, 2].Value = "First Name";
                    worksheet.Cells[1, 3].Value = "Last Name";
                    worksheet.Cells[1, 4].Value = "Number";
                    worksheet.Cells[1, 5].Value = "Mail";
                    worksheet.Cells[1, 6].Value = "Adresse";



                    worksheet.Cells[ 2, 1].Value = knowncustomer[i].IdPerson;
                    worksheet.Cells[ 2, 1].Style.HorizontalAlignment = OfficeOpenXml.Style.ExcelHorizontalAlignment.Center;
                    worksheet.Cells[ 2, 2].Value = knowncustomer[i].IdPersonNavigation.FirstName;
                    worksheet.Cells[ 2, 3].Value = knowncustomer[i].IdPersonNavigation.LastName;
                    worksheet.Cells[ 2, 4].Value = knowncustomer[i].IdPersonNavigation.Number;
                    worksheet.Cells[ 2, 5].Value = knowncustomer[i].IdPersonNavigation.Mail;
                    worksheet.Cells[ 2, 6].Value = knowncustomer[i].IdPersonNavigation.Address;


                    var requestList = knowcustomerItem.Requests.Where(d => d.IdPerson == knowcustomerItem.IdPerson).ToList();

                    worksheet.Cells[3, 1].Value = "Request Id";
                    worksheet.Cells[3, 2].Value = "Species Asked";
                    worksheet.Cells[3, 3].Value = "Date Request";
                    worksheet.Cells[3, 4].Value = "Quantity Asked";
                    worksheet.Cells[3, 5].Value = "Price";
                    worksheet.Cells[3, 6].Value = "Person ID";
                    worksheet.Cells[3, 7].Value = "Status Request";
                    

                    int numbercell = 4;
                    foreach (var plant in requestList)
                    {
                        worksheet.Cells[numbercell, 1].Value = plant.IdRequest;
                        worksheet.Cells[numbercell, 2].Value = plant.SpeciesAsked;
                        worksheet.Cells[numbercell, 3].Value = plant.DateRequest;
                        worksheet.Cells[numbercell, 4].Value = plant.QuantityAsked;
                        worksheet.Cells[numbercell, 5].Value = plant.PriceAsked;
                        worksheet.Cells[numbercell, 6].Value = plant.IdPerson;
                        worksheet.Cells[numbercell, 7].Value = plant.StatusRequest;
                        
                        numbercell++;
                    }
                }



                content = excel.GetAsByteArray();
            }

            return File(content, ExcelContentType, "KnownCustomerMD.xlsx");

        }











        public async Task<IActionResult> Requestonly()
        {
            string title = "Request";
            var request = await ctx.Requests
                                .AsNoTracking()
                                .OrderBy(d => d.IdRequest)
                                .ToListAsync();
            PdfReport report = CreateReport("Request");
            report.PagesFooter(footer => { footer.DefaultFooter(DateTime.Now.ToString("dd.MM,yyyy")); })
                    .PagesHeader(header => {
                        header.CacheHeader(cache: true);
                        header.DefaultHeader(defaultHeader => {
                            defaultHeader.RunDirection(PdfRunDirection.LeftToRight);
                            defaultHeader.Message(title);
                        });
                    });
            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(request));
            report.MainTableColumns(columns => {
                columns.AddColumn(column => {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true); column.Order(0); column.Width(1);
                    column.HeaderCell("#",
                    horizontalAlignment: HorizontalAlignment.Right);
                });
                columns.AddColumn(column => {
                    column.PropertyName<Request>(x => x.IdRequest);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(1); column.Width(2);
                    column.HeaderCell("Request ID");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Request>(x => x.SpeciesAsked);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(2); column.Width(2);
                    column.HeaderCell("Species Asked");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Request>(x => x.DateRequest);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(3); column.Width(3);
                    column.HeaderCell("Date Request");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Request>(x => x.QuantityAsked);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(4); column.Width(4);
                    column.HeaderCell("Quantity Asked");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Request>(x => x.PriceAsked);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(5); column.Width(4);
                    column.HeaderCell("Price Asked");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Request>(x => x.IdPerson);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(6); column.Width(2);
                    column.HeaderCell("Id Person");
                });
                columns.AddColumn(column => {
                    column.PropertyName<Request>(x => x.StatusRequest);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Order(7); column.Width(2);
                    column.HeaderCell("Status Request");
                });
            });

            byte[] pdf = report.GenerateAsByteArray();
            if (pdf != null)
            {
                Response.Headers.Add("content-disposition",
                "inline; filename=sale.pdf");
                return File(pdf, "application/pdf");
            }
            else return NotFound();
        }

        public async Task<IActionResult> Plot()
        {
                int n = 10;
                string title = $"Plot with all his plant";
            var plants = await ctx.BiggestPurchases(n)
            .OrderBy(s => s.IdPlot)
            .ThenBy(s => s.IdPlant)
            .ToListAsync();
            var param = new SqlParameter("@N", 10);
            //var plants  = await ctx.PlantsDenorm.FromSqlRaw($"Select TOP {n} * FROM PlantsDenorm")
            //                            .OrderBy(s => s.IdPlot)
            //                            .ThenBy(s => s.IdPlant)
            //                            .ToListAsync();
            


            //var plants = ctx.TenFirstPlotResults
            //               .OrderBy(s => s.IdPlot)
            //               .ThenBy(s => s.IdPlant)
            //               .Take(10)
            //               .ToList();
            plants.ForEach(s =>
            {
                s.PlotUrl = Url.Action("Edit", "Plot", new { id = s.IdPlot });
            });

            PdfReport report = CreateReport(title);
            report.MainTableColumns(columns => {
                columns.AddColumn(column => {
                    column.PropertyName("PlotId");
                    column.Group(
                    (val1, val2) =>
                    {
                        return (int)val1 == (int)val2;
                    });
                });

            });

            report.PagesHeader(header => {
                header.CacheHeader(cache: true);
                header.CustomHeader(new MasterDetailsHeaders(title)
                {
                    PdfRptFont = header.PdfFont
                });
            });

            report.MainTableDataSource(dataSource => dataSource.StronglyTypedList(plants));

            report.MainTableSummarySettings(summarySettings =>
            {
                summarySettings.OverallSummarySettings("Total");
            });

            report.MainTableColumns(columns =>
            {
                columns.AddColumn(column =>
                {
                    column.PropertyName<PlantsDenorm>(s => s.IdPlot);
                    column.Group(
                        (val1, val2) =>
                        {
                            return (int)val1 == (int)val2;
                        });
                });
                columns.AddColumn(column =>
                {
                    column.IsRowNumber(true);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Width(1);
                    column.HeaderCell("#", horizontalAlignment: HorizontalAlignment.Right);
                });
                columns.AddColumn(column =>
                {
                    column.PropertyName<PlantsDenorm>(x => x.Species);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Center);
                    column.IsVisible(true);
                    column.Width(4);
                    column.HeaderCell("Species Name", horizontalAlignment: HorizontalAlignment.Center);
                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PlantsDenorm>(x => x.Quantity);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Width(1);
                    column.HeaderCell("Quantity", horizontalAlignment: HorizontalAlignment.Center);

                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PlantsDenorm>(x => x.SpeciesGroup);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Width(1);
                    column.HeaderCell("Species Group", horizontalAlignment: HorizontalAlignment.Center);

                });

                columns.AddColumn(column =>
                {
                    column.PropertyName<PlantsDenorm>(x => x.FruitVegetable);
                    column.CellsHorizontalAlignment(HorizontalAlignment.Right);
                    column.IsVisible(true);
                    column.Width(1);
                    column.HeaderCell("Fruit or Vegetable", horizontalAlignment: HorizontalAlignment.Center);

                });


            });


            byte[] pdf = report.GenerateAsByteArray();

            if (pdf != null)
            {
                Response.Headers.Add("content-disposition", "inline; filename=documents.pdf");
                return File(pdf, "application/pdf");
            }
            else
                return NotFound();

        }


        public class MasterDetailsHeaders : IPageHeader
        {
            private readonly string title;
            public MasterDetailsHeaders(string title)
            {
                this.title = title;
            }
            public IPdfFont PdfRptFont { set; get; }

            public PdfGrid RenderingGroupHeader(iTextSharp.text.Document pdfDoc, PdfWriter pdfWriter, IList<CellData> newGroupInfo, IList<SummaryCellData> summaryData)
            {
                var IdPlot = newGroupInfo.GetSafeStringValueOf(nameof(PlantsDenorm.IdPlot));
                var PlotUrl = newGroupInfo.GetSafeStringValueOf(nameof(PlantsDenorm.PlotUrl));
                var PlotName = newGroupInfo.GetSafeStringValueOf(nameof(PlantsDenorm.PlotName));
                var Infrastructure = newGroupInfo.GetSafeStringValueOf(nameof(PlantsDenorm.Infrastructure));
                var SunIntensity = (decimal)newGroupInfo.GetValueOf(nameof(PlantsDenorm.SunIntensity));
                var PlotGps = newGroupInfo.GetSafeStringValueOf(nameof(PlantsDenorm.PlotGps));
                var IdSoil = newGroupInfo.GetSafeStringValueOf(nameof(PlantsDenorm.IdSoil));
                var IdLease = newGroupInfo.GetSafeStringValueOf(nameof(PlantsDenorm.IdLease));

                var table = new PdfGrid(relativeWidths: new[] { 2f, 5f, 2f, 3f }) { WidthPercentage = 100 };

                table.AddSimpleRow(
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Plot Id::";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.TableRowData = newGroupInfo;
                        var cellTemplate = new HyperlinkField(BaseColor.Black, false)
                        {
                            TextPropertyName = nameof(PlantsDenorm.IdPlot),
                            NavigationUrlPropertyName = nameof(PlantsDenorm.PlotUrl),
                            BasicProperties = new CellBasicProperties
                            {
                                HorizontalAlignment = HorizontalAlignment.Left,
                                PdfFontStyle = DocumentFontStyle.Bold,
                                PdfFont = PdfRptFont
                            }
                        };

                        cellData.CellTemplate = cellTemplate;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Plot name:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = PlotName;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;

                    });

                table.AddSimpleRow(
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Infrastructure:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = Infrastructure;
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = "Sun Intensity:";
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    },
                    (cellData, cellProperties) =>
                    {
                        cellData.Value = SunIntensity;
                        cellProperties.DisplayFormatFormula = obj => ((decimal)obj).ToString("C2");
                        cellProperties.PdfFont = PdfRptFont;
                        cellProperties.HorizontalAlignment = HorizontalAlignment.Left;
                    });
                return table.AddBorderToTable(borderColor: BaseColor.LightGray, spacingBefore: 5f);
            }

            public PdfGrid RenderingReportHeader(iTextSharp.text.Document pdfDoc, PdfWriter pdfWriter, IList<SummaryCellData> summaryData)
            {
                var table = new PdfGrid(numColumns: 1) { WidthPercentage = 100 };
                table.AddSimpleRow(
                   (cellData, cellProperties) =>
                   {
                       cellData.Value = title;
                       cellProperties.PdfFont = PdfRptFont;
                       cellProperties.PdfFontStyle = DocumentFontStyle.Bold;
                       cellProperties.HorizontalAlignment = HorizontalAlignment.Center;
                   });
                return table.AddBorderToTable();
            }

        }



    } 
}
