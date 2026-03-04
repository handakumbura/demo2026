using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Xunit.Priority;

namespace PlaywrightTests;

[TestCaseOrderer(PriorityOrderer.Name, PriorityOrderer.Assembly)]
public class Amoused : PageTest
{
    private static string? _createdObjectId;
    private async Task<IAPIRequestContext> MyTests()
    {
        string? API_TOKEN = "21c5d82a-96d4-4289-b456-587db9578036";

        var headers = new Dictionary<string, string>();
        headers.Add("Content-Type", "application/json");
        headers.Add("x-api-key", API_TOKEN);

        var requestContext = await this.Playwright.APIRequest.NewContextAsync(new()
        {
            BaseURL = "https://api.restful-api.dev/collections/products",
            ExtraHTTPHeaders = headers,
        });

        return requestContext;
    }

    [Fact, Priority(1)]
    public async Task AddObject()
    {
        var Request = await MyTests();
        // Read the payload.json file
        var payloadPath = Path.Combine(Directory.GetCurrentDirectory(), "AddObject.json");
        var jsonPayload = await File.ReadAllTextAsync(payloadPath);
        // Send the raw JSON as the request body
        var response = await Request.PostAsync("/objects", new() { Data = jsonPayload });
        await Expect(response).ToBeOKAsync();

        // Parse and assert response content
        var responseJson = await response.JsonAsync();
        Assert.NotNull(responseJson);
        Assert.True(responseJson.Value.TryGetProperty("id", out var idProp), "Response should contain 'id'");
        var id = idProp.GetString();
        Assert.False(string.IsNullOrEmpty(id), "'id' should not be null or empty");
        _createdObjectId = id;

        Assert.True(responseJson.Value.TryGetProperty("name", out var nameProp), "Response should contain 'name'");
        Assert.Equal("Dell Latitude 5421", nameProp.GetString());

        Assert.True(responseJson.Value.TryGetProperty("data", out var dataProp), "Response should contain 'data'");
        Assert.Equal(2019, dataProp.GetProperty("year").GetInt32());
        Assert.Equal(1849.99, dataProp.GetProperty("price").GetDouble());
        Assert.Equal("Intel Core i5", dataProp.GetProperty("CPU model").GetString());
        Assert.Equal("1 TB", dataProp.GetProperty("Hard disk size").GetString());
    }

    [Fact, Priority(2)]
    public async Task GetObject()
    {
        var Request = await MyTests();
        var get = await Request.GetAsync($"/objects/{_createdObjectId}");
        await Expect(get).ToBeOKAsync();

        // Parse and assert response content
        var responseJson = await get.JsonAsync();
        Assert.NotNull(responseJson);
        Assert.True(responseJson.Value.TryGetProperty("name", out var nameProp), "Response should contain 'name'");
        Assert.Equal("Dell Latitude 5421", nameProp.GetString());
        Assert.True(responseJson.Value.TryGetProperty("data", out var dataProp), "Response should contain 'data'");
        Assert.Equal(2019, dataProp.GetProperty("year").GetInt32());
        Assert.Equal(1849.99, dataProp.GetProperty("price").GetDouble());
        Assert.Equal("Intel Core i5", dataProp.GetProperty("CPU model").GetString());
        Assert.Equal("1 TB", dataProp.GetProperty("Hard disk size").GetString());
    }

    [Fact, Priority(3)]
    public async Task GetObjects()
    {
        var Request = await MyTests();
        var get = await Request.GetAsync($"/objects");
        await Expect(get).ToBeOKAsync();

         // Parse and assert response content
        var responseJson = await get.JsonAsync();
        Assert.NotNull(responseJson);  
    }

    [Fact, Priority(4)]
    public async Task UpdateObject()
    {
        var Request = await MyTests();
        // Read the payload.json file
        var payloadPath = Path.Combine(Directory.GetCurrentDirectory(), "UpdateObject.json");
        var jsonPayload = await File.ReadAllTextAsync(payloadPath);
        // Send the raw JSON as the request body
        var response = await Request.PutAsync($"/objects/{_createdObjectId}", new() { Data = jsonPayload });
        await Expect(response).ToBeOKAsync();

        // Parse and assert response content
        var responseJson = await response.JsonAsync();
        Assert.NotNull(responseJson);
        var id = responseJson?.GetProperty("id").GetString();
        _createdObjectId = id;

        // Check for expected properties
        Assert.Equal("Dell Latitude 5521", responseJson?.GetProperty("name").GetString());
        var dataProp = responseJson?.GetProperty("data");
        Assert.NotNull(dataProp);
        Assert.Equal(2020, dataProp?.GetProperty("year").GetInt32());
        Assert.Equal(2000.00, dataProp?.GetProperty("price").GetDouble());
        Assert.Equal("Intel Core i7", dataProp?.GetProperty("CPU model").GetString());
        Assert.Equal("1 TB", dataProp?.GetProperty("Hard disk size").GetString());
    }


    [Fact, Priority(5)]
    public async Task DeleteObject()
    {
        var Request = await MyTests();
        var delete = await Request.DeleteAsync($"/objects/{_createdObjectId}");
        await Expect(delete).ToBeOKAsync();
    }


}