import os
import openai

openai.api_key = os.getenv("OPENAI_API_KEY")

schema = {
  "type": "object",
  "properties": {
    "Company": {
      "type": "string",
      "description": "Name of the theater or production company"
    },
    "Performance": {
      "type": "string",
      "description": "Name of the show or performance"
    },
    "JobName": {
      "type": "string",
      "description": "Name of the job or role"
    },
    "Description": {
      "type": "string",
      "description": "Short description of the opportunity"
    },
    "Summary": {
      "type": "string",
      "description": "Full summary of the role"
    },
    "AuditionDateTime": {
      "type": "string",
      "format": "date-time",
      "description": "Audition start date and time in ISO 8601 format"
    },
    "AuditionEndDateTime": {
      "type": "string",
      "format": "date-time",
      "description": "Audition end date and time in ISO 8601 format"
    }
  },
  "required": ["Company", "Type", "ShowName", "JobName", "Description", "Summary", "AuditionDateTime"]
}

completion = openai.ChatCompletion.create(
  model="gpt-4-0613",
  messages=[
    {"role": "system", "content": "You are a helpful assistant who finds and organizes auditions and performance opportunities."},
    {"role": "user", "content": "Provide a opportunity for each found in {contentThatChanged}"}
  ],
  functions=[{"name": "create_opportunity", "parameters": schema}],
  function_call={"name": "create_opportunity"},
  temperature=0,
)

print(completion.choices[0].message.function_call.arguments)