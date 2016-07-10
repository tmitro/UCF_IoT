/**
 * Main Lambda function script for handling interaction with the Intelligent Home skill.
 */
 
'use strict';

// Route the incoming request based on type (LaunchRequest, IntentRequest,
// etc.) The JSON body of the request is provided in the event parameter.
exports.handler = function (event, context) {
    try {
        console.log("event.session.application.applicationId=" + event.session.application.applicationId);

        /**
         * Uncomment this if statement and populate with your skill's application ID to
         * prevent someone else from configuring a skill that sends requests to this function.
         */

//     if (event.session.application.applicationId !== "amzn1.echo-sdk-ams.app.05aecccb3-1461-48fb-a008-822ddrt6b516") {
//         context.fail("Invalid Application ID");
//      }

        if (event.session.new) {
            onSessionStarted({requestId: event.request.requestId}, event.session);
        }

        if (event.request.type === "LaunchRequest") {
            onLaunch(event.request,
                event.session,
                function callback(sessionAttributes, speechletResponse) {
                    context.succeed(buildResponse(sessionAttributes, speechletResponse));
                });
        } else if (event.request.type === "IntentRequest") {
            onIntent(event.request,
                event.session,
                function callback(sessionAttributes, speechletResponse) {
                    context.succeed(buildResponse(sessionAttributes, speechletResponse));
                });
        } else if (event.request.type === "SessionEndedRequest") {
            onSessionEnded(event.request, event.session);
            context.succeed();
        }
    } catch (e) {
        context.fail("Exception: " + e);
    }
};

/**
 * Called when the session starts.
 */
function onSessionStarted(sessionStartedRequest, session) {
    console.log("onSessionStarted requestId=" + sessionStartedRequest.requestId
        + ", sessionId=" + session.sessionId);

    // add any session init logic here
}

/**
 * Called when the user invokes the skill without specifying what they want.
 */
function onLaunch(launchRequest, session, callback) {
    console.log("onLaunch requestId=" + launchRequest.requestId
        + ", sessionId=" + session.sessionId);

    getWelcomeResponse(callback);
}

/**
 * Called when the user specifies an intent for this skill.
 */
function onIntent(intentRequest, session, callback) {
    console.log("onIntent requestId=" + intentRequest.requestId
        + ", sessionId=" + session.sessionId);

    var intent = intentRequest.intent,
        intentName = intentRequest.intent.name;

    // handle yes/no intent after the user has been prompted
    if (session.attributes && session.attributes.userPromptedToContinue) {
        delete session.attributes.userPromptedToContinue;
        if ("AMAZON.NoIntent" === intentName) {
            handleFinishSessionRequest(intent, session, callback);
        }
    }

    // dispatch custom intents to handlers here
    if ("ChangeModelIntent" === intentName) {
        handleChangeModelRequest(intent, session, callback);
    } else if ("RotateCameraIntent" === intentName) {
        handleRotateCameraRequest(intent, session, callback);
    } else if ("AMAZON.HelpIntent" === intentName) {
        handleGetHelpRequest(intent, session, callback);
    } else if ("AMAZON.StopIntent" === intentName) {
        handleFinishSessionRequest(intent, session, callback);
    } else if ("AMAZON.CancelIntent" === intentName) {
        handleFinishSessionRequest(intent, session, callback);
    } else {
        throw "Invalid intent";
    }
}

/**
 * Called when the user ends the session.
 * Is not called when the skill returns shouldEndSession=true.
 */
function onSessionEnded(sessionEndedRequest, session) {
    console.log("onSessionEnded requestId=" + sessionEndedRequest.requestId
        + ", sessionId=" + session.sessionId);

    // Add any cleanup logic here
}

// ------- Skill specific business logic -------

var AIO_API_URL = 'io.adafruit.com';
var AIO_KEY = 'b7ee7c04e9214aeab86eae82abc14730';
var AIO_FEED_KEY = '596538'//'intelligent-home';

const http = require('http');

function getWelcomeResponse(callback) {
    var speechOutput = "Welcome to the intelligent home, how can I assist you?";
    callback({}, buildSpeechletResponseWithoutCard(speechOutput, false));
}

function handleChangeModelRequest(intent, session, callback) {
	
	//var payload = buildModelPayload(intent.slots.Model.value);
	console.log('handleChangeModelRequest, Model: ' + intent.slots.Model.value);
	var payload = '{"model": ' + intent.slots.Model.value + '}'; //JSON.stringify(payload);
	var body = '{"value": ' + payload +'}';
	var path = '/api/feeds/intelligent-home/data?x-aio-key=b7ee7c04e9214aeab86eae82a';
	
	var request = new http.ClientRequest({
		hostname: AIO_API_URL,
		path: path,
		method: 'POST',
		headers: {
			//"X-AIO-KEY": AIO_KEY,
			'Content-Type': 'application/json',
			'Content-Length': Buffer.byteLength(body)
		}
	});
	
	console.log('handleChangeModelRequest, body:\n ' + body + ', path: ', AIO_API_URL + path);
	request.end(body);
	
	request.on('response', function (response) {
		console.log('STATUS: ' + response.statusCode);
		console.log('HEADERS: ' + JSON.stringify(response.headers));
		response.setEncoding('utf8');
		response.on('data', function (chunk) {
			console.log('BODY: ' + chunk);
	  });
	});

    var speechOutput = "Model changed to " + intent.slots.Model.value + ".";
	callback({}, buildSpeechletResponseWithoutCard(speechOutput, true));
}

function handleRotateCameraRequest(intent, session, callback) {
	// TODO: parse response and post to Adafruit RESTful interace.
	var payload = buildCameraPayload(intent.slots.Direction.value);
	
    var speechOutput = "Camera rotated " + intent.slots.Direction.value + ".";
	callback({}, buildSpeechletResponseWithoutCard(speechOutput, true));
}

function handleGetHelpRequest(intent, session, callback) {
    // Provide a help prompt for the user, explaining how the game is played. Then, continue the game
    // if there is one in progress, or provide the option to start another one.

    // Set a flag to track that we're in the Help state.
    session.attributes.userPromptedToContinue = true;
    var speechOutput = "I can interact with smart devices or manipulate the virtual environment. What would you like to do?";
    callback(session.attributes, buildSpeechletResponseWithoutCard(speechOutput, false));
}

function handleFinishSessionRequest(intent, session, callback) {
    // End the session with a "Good bye!" if the user wants to quit the game
    callback(session.attributes,
        buildSpeechletResponseWithoutCard("", true));
}

// ------- Helper functions to build payloads and responses -------

function buildModelPayload(title, model, shouldEndSession) { 
	var obj = {};
	obj.model = model;
	return obj;
}

function buildCameraPayload(title, direction, shouldEndSession) {
	return {
        direction: direction
    };
}

function buildSpeechletResponse(title, output, shouldEndSession) {
    return {
        outputSpeech: {
            type: "PlainText",
            text: output
        },
        card: {
            type: "Simple",
            title: title,
            content: output
        },
        shouldEndSession: shouldEndSession
    };
}

function buildSpeechletResponseWithoutCard(output, shouldEndSession) {
    return {
        outputSpeech: {
            type: "PlainText",
            text: output
        },
        shouldEndSession: shouldEndSession
    };
}

function buildResponse(sessionAttributes, speechletResponse) {
    return {
        version: "1.0",
        sessionAttributes: sessionAttributes,
        response: speechletResponse
    };
}