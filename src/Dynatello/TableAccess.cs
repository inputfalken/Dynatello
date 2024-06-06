using DynamoDBGenerator;
using Dynatello.Handlers;

namespace Dynatello;

internal class TableAccess<T, TArg, TReferences, TArgumentReferences> : 
  ITableAccess<T, TArg, TReferences, TArgumentReferences>, 
  IRequestBuilder<T, TArg, TReferences, TArgumentReferences>
  where TReferences : IAttributeExpressionNameTracker
  where TArgumentReferences : IAttributeExpressionValueTracker<TArg>
{
    internal TableAccess(in string tableName, in IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> item)
    {
        TableName = tableName ?? throw new ArgumentNullException(nameof(tableName));
        Marshaller = item ?? throw new ArgumentNullException(nameof(item));
    }

    public IDynamoDBMarshaller<T, TArg, TReferences, TArgumentReferences> Marshaller { get; }

    public string TableName { get; }

    ITableAccess<T, TArg, TReferences, TArgumentReferences> IRequestBuilder<T, TArg, TReferences, TArgumentReferences>.TableAccess => this;
}
