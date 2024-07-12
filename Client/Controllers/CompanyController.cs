using Client.Interfaces;
using Client.Models;
using Client.Models.CompanyViewModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using Shared.Requests;
using Shared.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CarClient.Controllers
{

    [Route("[controller]")]
    [Controller]
    public class CompanyController : Controller
    {
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly IClientMessageHub _serverMessageHub;

        public CompanyController(SignInManager<ApplicationUser> signInManager, IClientMessageHub serverMessageHub)
        {
            _signInManager = signInManager;
            _serverMessageHub = serverMessageHub;
        }


        [HttpGet("/company")]

        public async Task<IActionResult> Index()
        {
            if (!_signInManager.IsSignedIn(User)) return RedirectToAction("Index", "Home");

            var correlationId = Guid.NewGuid();
            await _serverMessageHub.SendToServerMessage(new GetCompaniesRequest(), correlationId);
            var getCompaniesResponse = await _serverMessageHub.ReceiveFromServerMessage<GetCompaniesResponse>(correlationId);

            var companies = getCompaniesResponse.Companies;

            foreach (var company in companies)
            {
                correlationId = Guid.NewGuid();
                await _serverMessageHub.SendToServerMessage(new GetCarsRequest(), correlationId);
                var getCarsResponse = await _serverMessageHub.ReceiveFromServerMessage<GetCarsResponse>(correlationId);

                var cars = getCarsResponse.Cars;
                cars = cars.Where(c => c.CompanyId == company.Id).ToList();
                company.Cars = cars;
            }

            var companyViewModel = new CompanyViewModel { Companies = companies };

            return View(companyViewModel);
        }


        [HttpGet("/company/details")]
        public async Task<IActionResult> Details(Guid id)
        {
            var correlationId = Guid.NewGuid();
            await _serverMessageHub.SendToServerMessage(new GetCompanyRequest(id), correlationId);
            var getCompanyResponse = await _serverMessageHub.ReceiveFromServerMessage<GetCompanyResponse>(correlationId);

            var company = getCompanyResponse.Company;

            return View(company);
        }

        [HttpGet("/company/create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Company/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/company/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Address,CreationTime")] Company company)
        {
            if (!ModelState.IsValid) return View(company);
            company.Id = Guid.NewGuid();

            var correlationId = Guid.NewGuid();
            await _serverMessageHub.SendToServerMessage(new CreateCompanyRequest(company), correlationId);
            var creteCompanyResponse = await _serverMessageHub.ReceiveFromServerMessage<CreateCompanyResponse>(correlationId);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/company/edit")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var correlationId = Guid.NewGuid();
            await _serverMessageHub.SendToServerMessage(new GetCompanyRequest(id), correlationId);
            var getCompanyResponse = await _serverMessageHub.ReceiveFromServerMessage<GetCompanyResponse>(correlationId);

            var company = getCompanyResponse.Company;

            return View(company);
        }

        // POST: Company/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/company/edit")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,CreationTime, Name, Address")] Company company)
        {
            if (!ModelState.IsValid) return View(company);

            var correlationId = Guid.NewGuid();
            await _serverMessageHub.SendToServerMessage(new GetCompanyRequest(id), correlationId);
            var getCompanyResponse = await _serverMessageHub.ReceiveFromServerMessage<GetCompanyResponse>(correlationId);

            var oldCompany = getCompanyResponse.Company;

            oldCompany.Name = company.Name;
            oldCompany.Address = company.Address;

            correlationId = Guid.NewGuid();
            await _serverMessageHub.SendToServerMessage(new UpdateCompanyRequest(company), correlationId);
            var updateCompanyResponse = await _serverMessageHub.ReceiveFromServerMessage<UpdateCompanyResponse>(correlationId);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet("/company/delete")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var correlationId = Guid.NewGuid();
            await _serverMessageHub.SendToServerMessage(new GetCompanyRequest(id), correlationId);
            var getCompanyResponse = await _serverMessageHub.ReceiveFromServerMessage<GetCompanyResponse>(correlationId);

            var company = getCompanyResponse.Company;

            return View(company);
        }

        // POST: Company/Delete/5
        [HttpPost("/company/delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var correlationId = Guid.NewGuid();
            await _serverMessageHub.SendToServerMessage(new DeleteCompanyRequest(id), correlationId);
            var deleteCompanyResponse = await _serverMessageHub.ReceiveFromServerMessage<DeleteCompanyResponse>(correlationId);

            return RedirectToAction(nameof(Index));
        }
    }
}