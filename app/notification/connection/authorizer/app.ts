// Copyright 2018-2020 Amazon.com, Inc. or its affiliates. All Rights Reserved.
// SPDX-License-Identifier: MIT-0

// https://docs.aws.amazon.com/apigateway/latest/developerguide/apigateway-websocket-api-route-keys-connect-disconnect.html
// The $disconnect route is executed after the connection is closed.
// The connection can be closed by the server or by the client. As the connection is already closed when it is executed, 
// $disconnect is a best-effort event. 
// API Gateway will try its best to deliver the $disconnect event to your integration, but it cannot guarantee delivery.

import { APIGatewayRequestAuthorizerHandler } from "aws-lambda";
import { CognitoJwtVerifier } from "aws-jwt-verify";


const UserPoolId = process.env.USER_POOL_ID;
const AppClientId = process.env.APP_CLIENT_ID;

export const lambdaHandler: APIGatewayRequestAuthorizerHandler = async (event, context) => {
    console.log(event);

    try {
        const verifier = CognitoJwtVerifier.create({
            userPoolId: UserPoolId,
            tokenUse: "id",
            clientId: AppClientId,
        });

        const encodedToken = event.queryStringParameters!.idToken!;
        console.log("token:", encodedToken);
        const payload = await verifier.verify(encodedToken);
        console.log("Token is valid. Payload:", payload);

        return allowPolicy(event.methodArn, payload);
    } catch (error) {
        console.log("Token invalid", error.message);
        return denyAllPolicy();
    }
};

const denyAllPolicy = () => {
    return {
        principalId: "*",
        policyDocument: {
            Version: "2012-10-17",
            Statement: [
                {
                    Action: "*",
                    Effect: "Deny",
                    Resource: "*",
                },
            ],
        },
    };
};

const allowPolicy = (methodArn, idToken) => {
    return {
        principalId: idToken.sub,
        policyDocument: {
            Version: "2012-10-17",
            Statement: [
                {
                    Action: "execute-api:Invoke",
                    Effect: "Allow",
                    Resource: methodArn,
                },
            ],
        },
        context: {
            // set userId in the context
            userId: idToken.sub,
        },
    };
};