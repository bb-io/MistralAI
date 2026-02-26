# Blackbird.io Mistral AI

Blackbird is the new automation backbone for the language technology industry. Blackbird provides enterprise-scale automation and orchestration with a simple no-code/low-code platform. Blackbird enables ambitious organizations to identify, vet, and automate as many processes as possible. Not just localization workflows, but any business and IT process. This repository represents an application that is deployable on Blackbird and usable inside the workflow editor.

## Introduction

<!-- begin docs -->

Fast, open-source and secure language models. Facilitated specialisation of models on business use-cases, leveraging private data and usage feedback.

## Before setting up

Before you can connect you need to make sure that:

- You must [have access to the Mistral AI console](https://console.mistral.ai/).
- Create your [API key](https://console.mistral.ai/api-keys/).
- Save this key for future use.

You can view a [complete guide](https://docs.mindmac.app/how-to.../add-api-key/create-mistral-ai-api-key) about creating the Mistral AI API Key with the registration process.

## Connecting 

1. Click _Add Connection_.
2. Name your connection for future reference, e.g., 'My client'.
3. In the _API Key_ field, input your API Key. You can get it from [here](https://console.mistral.ai/api-keys/).
4. Click _Connect_.
5. Confirm that the connection has appeared and the status is _Connected_.

![MistralAIConnection](image/README/MistralAIConnection.png)

## Actions

### Chat

- **Send prompt**: This action allows you to send a prompt to an AI model and receive its response. When you submit your message, it's processed and sent to the AI through our system. The AI then generates a response based on your input. If you've had a conversation history, it will be preserved and included along with the new AI response in the returned details

### OCR

- **Extract text from image or PDF** Extracts text from a PDF or image file using Mistral AI OCR service.

### Audio
- **Create transcription**: generates a transcription given an audio (mp3, mpeg, mpga, m4a, wav, or webm).

### Translation

- **Translate** translate file content retrieved from a CMS or file storage. The output can be used in compatible Blackbird interoperable actions.
- **Translate text** given a text and a locale, tries to create a localized version of the text.

### Editing

- **Edit** Edit a translation. This action assumes you have previously translated content in Blackbird through any translation action. Only looks at translated segments and will change the segment state to reviewed.
- **Edit Text** given a source segment and translated target segment, responds with an edited version of the target segment taking into account typical mistakes.

### Review

- **Review** reviews a translation. This action assumes you have previously translated content in Blackbird through any translation action. Adds a quality score to each unit in the file.
- **Review text** reviews a single text and returns a quality score.

> Note: files are interoperable, under the hood Blackbird may convert to and from bilingual files with metadata but all our Actions are able to accept a lot of different file formats.

## Example

Using our application you can build the following bird:

![MistralAIExample](image/README/MistralAIExample.png)

First action:

![MistralAIExample](image/README/MistralAIExample-1-action.png)

Second action:

![MistralAIExample](image/README/MistralAIExample-2-action.png)

Third action:

![MistralAIExample](image/README/MistralAIExample-3-action.png)

As you can see, you can specify a message history to maintain the context of the conversation

## Feedback

Do you want to use this app or do you have feedback on our implementation? Reach out to us using the [established channels](https://www.blackbird.io/) or create an issue.

<!-- end docs -->
