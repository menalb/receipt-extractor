const { SSMClient, GetParameterCommand } = require("@aws-sdk/client-ssm");
const { MongoClient } = require("mongodb");

// Settings come from AWS Systems Manager Parameter Store, never from a committed file:
// MONGO_CONNECTION_STRING_PARAM only names the SSM parameter that holds the real
// connection string (a SecureString), it is not the secret itself.
const ssm = new SSMClient({ region: process.env.AWS_REGION });

// Cached across warm invocations of the same Lambda execution environment, so we
// only hit SSM / open a Mongo connection once per cold start.
let cachedClientPromise = null;

async function fetchConnectionString() {
    const paramName = process.env.MONGO_CONNECTION_STRING_PARAM;
    const result = await ssm.send(new GetParameterCommand({ Name: paramName, WithDecryption: true }));
    return result.Parameter.Value;
}

async function connect() {
    const uri = await fetchConnectionString();
    const client = new MongoClient(uri);
    await client.connect();
    return client;
}

async function getConnectionsCollection() {
    if (!cachedClientPromise) {
        cachedClientPromise = connect();
    }
    const client = await cachedClientPromise;
    return client.db(process.env.MONGO_DATABASE_NAME).collection(process.env.MONGO_COLLECTION_NAME);
}

module.exports = { getConnectionsCollection };
