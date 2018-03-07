using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;

namespace functionsample
{
    public static class Functions
    {
        [FunctionName("AppStart")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            string name = req.GetQueryNameValuePairs()
         .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
         .Value;

            string age = req.GetQueryNameValuePairs()
         .FirstOrDefault(q => string.Compare(q.Key, "age", true) == 0)
         .Value;

            string phone = req.GetQueryNameValuePairs()
         .FirstOrDefault(q => string.Compare(q.Key, "phone", true) == 0)
         .Value;

            // Get request body
            dynamic data = await req.Content.ReadAsAsync<object>();

            // Set name to query string or body data
            name = name ?? data?.name;
            age = age ?? data?.age;
            phone = phone ?? data?.phone;

            var customer = new Customer()
            {
                Name = name,
                Age = age,
                Phone = phone
            };

            var msg = JsonConvert.SerializeObject(customer);

#if Debug

                var builder = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: false, reloadOnChange: true);
                IConfiguration Configuration = builder.Build();

            string con = Configuration["Values:AzureWebJobsStorage"];

#endif

            string con = System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");

            CloudQueueClient queueClient = CloudStorageAccount.Parse(con).CreateCloudQueueClient();

            CloudQueue queue = queueClient.GetQueueReference("customers");
            CloudQueueMessage message = new CloudQueueMessage(msg);
            queue.AddMessage(message);


            return name == null
                           ? req.CreateResponse(HttpStatusCode.BadRequest, "Please insert the folder name and the year that you want to proccess in the linker...")
                           : req.CreateResponse(HttpStatusCode.OK, "Function Started! " + "name is -" + name);

        }


        [FunctionName("Writer")]
        public static void Read([QueueTrigger("customers", Connection = "AzureWebJobsStorage")]string message, TraceWriter log)
        {
            //Deserialize customer from the queue
            Customer customer = JsonConvert.DeserializeObject<Customer>(message);

#if Debug
       

                 var builder = new ConfigurationBuilder()
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
                IConfiguration Configuration = builder.Build();

            string con = Configuration["Values:AzureWebJobsStorage"];
#endif

            string con = System.Environment.GetEnvironmentVariable("AzureWebJobsStorage");



            CloudBlobClient blobClient = CloudStorageAccount.Parse(con).CreateCloudBlobClient();

            var folder = "files";

            CloudBlobContainer container = blobClient.GetContainerReference(folder);
            container.CreateIfNotExists();

            CloudAppendBlob blob = container.GetAppendBlobReference("customers.txt");

            if (!blob.Exists())
            {
                blob.CreateOrReplace();
            }

            blob.AppendText(customer.Name + ";" + customer.Age + ";" + customer.Phone);
          
            // blob.UploadText(customer.Name + ";" + customer.Age + ";" + customer.Phone);

        }
    
    }
}
