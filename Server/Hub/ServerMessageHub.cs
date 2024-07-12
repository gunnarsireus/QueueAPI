using Newtonsoft.Json;
using Server.Interfaces;
using Shared.Helpers;
using Shared.Models;
using Shared.Repositories;
using Shared.Requests;
using Shared.Responses;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Hub;

public class ServerMessageHub : IServerMessageHub
{
    private readonly IQueueRepository _queueRepository;
    readonly ICompanyRepository _companyRepository;
    readonly ICarRepository _carRepository;

    public ServerMessageHub(ICompanyRepository companyRepository, ICarRepository carRepository, IQueueRepository queueRepository)
    {
        _companyRepository = companyRepository;
        _carRepository = carRepository;
        _queueRepository = queueRepository;
    }

    public async Task CheckForNewClientMessage()
    {
        while (true)
        {
            var nextPackage = await _queueRepository.GetMessageFromQlientQueue();
            if (nextPackage == null) break;

            await HandleMessageFormClient(nextPackage);
        }
    }

    private async Task HandleMessageFormClient(ClientQueueEntity queuePackage)
    {
        string[] classNameParts = queuePackage.TypeName.Split('.');
        string simpleClassName = classNameParts[^1];
        var requestMessage = JsonConvert.DeserializeObject(queuePackage.Content, Helpers.GetType(queuePackage.TypeName));

        Task<object> result = simpleClassName switch
        {
            nameof(CreateCarRequest) => HandleCreateCarRequest((CreateCarRequest)requestMessage).ContinueWith(task => (object)task.Result),
            nameof(CreateCompanyRequest) => HandleCreateCompanyRequest((CreateCompanyRequest)requestMessage).ContinueWith(task => (object)task.Result),
            nameof(DeleteCarRequest) => HandleDeleteCarRequest((DeleteCarRequest)requestMessage).ContinueWith(task => (object)task.Result),
            nameof(DeleteCompanyRequest) => HandleDeleteCompanyRequest((DeleteCompanyRequest)requestMessage).ContinueWith(task => (object)task.Result),
            nameof(GetCarRequest) => HandleGetCarRequest((GetCarRequest)requestMessage).ContinueWith(task => (object)task.Result),
            nameof(GetCarsRequest) => HandleGetCarsRequest((GetCarsRequest)requestMessage).ContinueWith(task => (object)task.Result),
            nameof(GetCompanyRequest) => HandleGetCompanyRequest((GetCompanyRequest)requestMessage).ContinueWith(task => (object)task.Result),
            nameof(GetCompaniesRequest) => HandleGetCompaniesRequest((GetCompaniesRequest)requestMessage).ContinueWith(task => (object)task.Result),
            nameof(UpdateCarRequest) => HandleUpdateCarRequest((UpdateCarRequest)requestMessage).ContinueWith(task => (object)task.Result),
            nameof(UpdateCompanyRequest) => HandleUpdateCompanyRequest((UpdateCompanyRequest)requestMessage).ContinueWith(task => (object)task.Result),
            _ => throw new NotSupportedException($"Request type {queuePackage.TypeName} is not supported.")
        };

        object actualResult = await result;
        await SendMessageToClient(actualResult, queuePackage.CorrelationId);
    }

    private async Task SendMessageToClient(object message, Guid correlationId)
    {
        var result = Helpers.ConvertObjectToJson(message);
        var entity = new ServerQueueEntity
        {
            CorrelationId = correlationId,
            Content = result.Item1,
            TypeName = result.Item2.ToString(),
            Created = DateTime.Now,
            StatusDate = DateTime.Now,
            QueueStatus = QueueStatus.New
        };

        await _queueRepository.AddServerQueueItemAsync(entity);
    }


    private async Task<CreateCarResponse> HandleCreateCarRequest(CreateCarRequest request)
    {
        await _carRepository.AddCarAsync(request.Car);

        var response = new CreateCarResponse()
        {
            DataId = request.DataId,
            Car = request.Car
        };

        return response;
    }

    private async Task<CreateCompanyResponse> HandleCreateCompanyRequest(CreateCompanyRequest request)
    {

        await _companyRepository.AddCompanyAsync(request.Company);

        var response = new CreateCompanyResponse()
        {
            DataId = request.DataId,
            Company = request.Company
        };

        return response;
    }

    private async Task<DeleteCarResponse> HandleDeleteCarRequest(DeleteCarRequest request)
    {

        await _carRepository.RemoveCarAsync(request.CarId);

        var response = new DeleteCarResponse()
        {
            DataId = request.DataId,
        };
        return response;
    }

    private async Task<DeleteCompanyResponse> HandleDeleteCompanyRequest(DeleteCompanyRequest request)
    {

        await _companyRepository.RemoveCompanyAsync(request.CompanyId);

        var response = new DeleteCompanyResponse()
        {
            DataId = request.DataId,
        };
        return response;
    }

    private async Task<GetCarResponse> HandleGetCarRequest(GetCarRequest request)
    {
        var car = await _carRepository.GetCarAsync(request.CarId);

        var response = new GetCarResponse()
        {
            DataId = request.DataId,
            Car = car
        };
        return response;
    }

    private async Task<GetCarsResponse> HandleGetCarsRequest(GetCarsRequest request)
    {
        var cars = await _carRepository.GetAllCarsAsync();

        var response = new GetCarsResponse()
        {
            DataId = request.DataId,
            Cars = cars.ToList()
        };

        return response;
    }

    private async Task<GetCompanyResponse> HandleGetCompanyRequest(GetCompanyRequest request)
    {
        var company = await _companyRepository.GetCompanyAsync(request.CompanyId);

        var response = new GetCompanyResponse()
        {
            DataId = request.DataId,
            Company = company
        };
        return response;
    }

    private async Task<GetCompaniesResponse> HandleGetCompaniesRequest(GetCompaniesRequest request)
    {
        try
        {
            var companies = await _companyRepository.GetAllCompaniesAsync();

            var response = new GetCompaniesResponse()
            {
                DataId = request.DataId,
                Companies = companies.ToList()
            };

            return response;
        }
        catch (Exception ex)
        {
            var a = ex.Message;
            throw;
        }
    }

    private async Task<UpdateCarResponse> HandleUpdateCarRequest(UpdateCarRequest request)
    {
        await _carRepository.UpdateCarAsync(request.Car);

        var response = new UpdateCarResponse()
        {
            DataId = request.DataId,
            Car = request.Car
        };

        return response;
    }

    private async Task<UpdateCompanyResponse> HandleUpdateCompanyRequest(UpdateCompanyRequest request)
    {

        await _companyRepository.UpdateCompanyAsync(request.Company);

        var response = new UpdateCompanyResponse()
        {
            DataId = request.DataId,
            Company = request.Company
        };
        return response;
    }
}
