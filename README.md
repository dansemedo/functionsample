# functionsample
A simple demo of Azure Functions that receives a HTTP Requests with some parameters, send to a queue and save it in a .csv Blob file.
## Como executar
### 1-	Clone o repositório https://github.com/dansemedo/functionsample ;
### 2-	Crie no Azure uma Function App (Consumption Plan);
### 3-	Associe o deploy com sua conta/branch no github;
### 4-	Insira no AppSettings no campo AzureWebJobsStorage a connectionstring do seu Azure Storage Account (esta demo necessita de Queue e Blob);
### 5-	Execute no Postman ou no portal do Azure a seguinte requisição na Function AppStart:;
 
http://localhost:7071/api/AppStart
{
    "name": "daniel",
    "age": "32",
    "phone": "11-2312-2312"
}
### 6-A Function deve funcionar, caso não funcione, me avise.
