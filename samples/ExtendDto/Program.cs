using DynamoDBGenerator.Attributes;
using Dynatello;
using Dynatello.Builders;
using Dynatello.Handlers;

IRequestHandler<string, Cat?> getById = Cat
    .FromId.OnTable("TABLE")
    .ToGetRequestHandler(x => x.ToGetRequestBuilder());

if (args.Length == 1)
{
    Cat? response = await getById.Send(args[0], CancellationToken.None);

    Console.WriteLine(response);
}
else
{
    throw new NotImplementedException();
}

[DynamoDBMarshaller(AccessName = "FromId", ArgumentType = typeof(string))]
public partial record Cat(string Id, string Name, double Cuteness);
