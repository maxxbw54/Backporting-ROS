import sys
import openai
import os
import yaml

RQ=3

config_path = os.path.join(os.path.dirname(os.path.dirname(__file__)), "config.yml")
with open(config_path, "r") as f:
    config = yaml.safe_load(f)

DATA_FOLDER = config["data_folder"]
if not DATA_FOLDER:
    raise ValueError("Data folder path is not specified in the config file.")

# Load OpenAI API key
openai.api_key = config.get("api_key")
if not openai.api_key:
    raise ValueError("API key is not specified in the config file.")

# Path to your training and validation data
training_file_path = config.get("rq{}_train".format(RQ))
if not training_file_path:
    raise ValueError("Training file path is not specified in the config file.")

validation_file_path = config.get("rq{}_validation".format(RQ))
if not validation_file_path:
    raise ValueError("Validation file path is not specified in the config file.")

# Upload the training file
with open(training_file_path, "rb") as f:
    train_file_response = openai.files.create(
        file=f,
        purpose="fine-tune"
    )
training_file_id = train_file_response.id

# Upload the validation file
with open(validation_file_path, "rb") as f:
    valid_file_response = openai.files.create(
        file=f,
        purpose="fine-tune"
    )
validation_file_id = valid_file_response.id

# Start fine-tuning (for chat models like gpt-3.5-turbo) with validation data
fine_tune_response = openai.fine_tuning.jobs.create(
    training_file=training_file_id,
    validation_file=validation_file_id,
    model="gpt-4.1-mini-2025-04-14"
)

print("Fine-tune job created:")
print(fine_tune_response)
