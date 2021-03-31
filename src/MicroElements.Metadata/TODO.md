#TODO
- SessionLogger : ILogger
- Message move from Functional to Metadata, use IPropertyContainer for properties
- Error move from Functional to Metadata

Template Engines:
- MessageTemplates
- ConfigurationTemplates
- Go template
- StringTemplate
- Fluid: https://github.com/sebastienros/fluid
- CSharpScript


IMessageTemplateRenderer - renders MessageTemplate to textWriter

Message       uses MessageTemplates          Example: "Session does not exists. SessionId: {sessionId}."
Configuration uses ConfigurationTemplates    Example: "SessionId: ${sessionId}.", "${env:profile}", "${expression:System.Environment.ProcessorCount}"
