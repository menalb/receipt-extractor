const { getConnectionsCollection } = require("./mongoConnections");

exports.handler = async event => {
    let userId = "";
    if (event.requestContext.authorizer) {
        userId = event.requestContext.authorizer.userId;
    }

    try {
        const connections = await getConnectionsCollection();
        await connections.replaceOne(
            { _id: event.requestContext.connectionId },
            { _id: event.requestContext.connectionId, userId },
            { upsert: true }
        );
    } catch (err) {
        return { statusCode: 500, body: 'Failed to connect: ' + JSON.stringify(err) };
    }

    return { statusCode: 200, body: 'Connected.' };
};
