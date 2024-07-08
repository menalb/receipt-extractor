const { DynamoDBClient } = require("@aws-sdk/client-dynamodb");
const { DynamoDBDocumentClient, PutCommand } = require("@aws-sdk/lib-dynamodb");

const client = new DynamoDBClient({ region: process.env.AWS_REGION });
// const ddb = new AWS.DynamoDB.DocumentClient({ apiVersion: '2012-08-10', region: process.env.AWS_REGION });
const ddb = DynamoDBDocumentClient.from(client);

exports.handler = async event => {
    let userId = "";
    if (event.requestContext.authorizer) {
        userId = event.requestContext.authorizer.userId;
    }
    const putParams = new PutCommand({
        TableName: process.env.TABLE_NAME,
        Item: {
            connectionId: event.requestContext.connectionId,
            userId: userId
        }
    });

    try {
        await ddb.send(putParams);
    } catch (err) {
        return { statusCode: 500, body: 'Failed to connect: ' + JSON.stringify(err) };
    }

    return { statusCode: 200, body: 'Connected.' };
};
