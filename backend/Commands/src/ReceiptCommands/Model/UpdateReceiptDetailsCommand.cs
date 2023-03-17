using System.Collections.Generic;
using Amazon.DynamoDBv2.DataModel;

namespace ReceiptCommand.Model;

[DynamoDBTable("receipts-items")]
public class SaveReceiptDetails
{        
    [DynamoDBProperty("receipt_id")]
    [DynamoDBRangeKey]
    public string Id { get; set; }
    [DynamoDBProperty("user_id")]
    [DynamoDBHashKey]
    public string UserId { get; set; }
    [DynamoDBProperty("job_id")]
    public string JobId { get; set; }
    [DynamoDBProperty("day")]
    public string Day { get; set; }
    [DynamoDBProperty("shop")]
    public UpdateShop Shop { get; set; }
    [DynamoDBProperty("total")]
    public decimal? Total { get; set; }
    [DynamoDBProperty("total_vat")]
    public decimal? TotalVAT { get; set; }
    [DynamoDBProperty("items")]
    public List<UpdateReceiptItem> Items { get; set; }
    [DynamoDBProperty("tags")]
    public List<string> Tags { get; set; }
    [DynamoDBProperty("notes")]
    public string Notes { get; set; }
}

public class UpdateShop
{
    [DynamoDBProperty("name")]
    public string Name { get; set; }
    [DynamoDBProperty("owner")]
    public string Owner { get; set; }
    [DynamoDBProperty("vat")]
    public string Vat { get; set; }
    [DynamoDBProperty("phone")]
    public string Phone { get; set; }
    [DynamoDBProperty("address")]
    public string Address { get; set; }
    [DynamoDBProperty("city")]
    public string City { get; set; }
}

public class UpdateReceiptItem
{
    [DynamoDBProperty("name")]
    public string Name { get; set; }
    [DynamoDBProperty("vat")]
    public decimal? VAT { get; set; }
    [DynamoDBProperty("price")]
    public decimal? Price { get; set; }
}