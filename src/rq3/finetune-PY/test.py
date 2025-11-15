import openai
import json
import os
import sys
sys.path.insert(0, os.path.join(os.path.dirname(__file__), '..', '..'))

import yaml
from sklearn.metrics import precision_score, recall_score, f1_score
from sklearn.metrics import classification_report

RQ=3





def eval(y_true, y_pred):
    precision = precision_score(y_true, y_pred, average="macro")
    recall = recall_score(y_true, y_pred, average="macro")
    f1 = f1_score(y_true, y_pred, average="macro")
    # Calculate metrics (macro average)
    print("Precision:", precision)
    print("Recall:   ", recall)
    print("F1 Score: ", f1)
    print("\nDetailed classification report:")
    print(classification_report(y_true, y_pred))
    return precision, recall, f1

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

finetuned_model_id = config.get("rq{}_fine_tune_model_id".format(RQ))
if not finetuned_model_id:
    raise ValueError("Finetuned model ID is not specified in the config file.")

test_file_path = config.get("rq{}_test".format(RQ))
if not test_file_path:
    raise ValueError("Test file path is not specified in the config file.")

y_true = []
y_pred = []

with open(test_file_path, "r") as test_file:
    for line in test_file:
        instance = json.loads(line)
        messages = instance["messages"]
        # Extract true label from assistant message
        true_label = None
        for msg in messages:
            if msg["role"] == "assistant":
                true_label = msg["content"].strip()
        if true_label is None:
            continue
        # Remove assistant message for prediction
        input_messages = [msg for msg in messages if msg["role"] != "assistant"]
        response = openai.chat.completions.create(
            model=finetuned_model_id,
            messages=input_messages
        )
        predicted_label = response.choices[0].message.content.strip()
        y_true.append(true_label)
        y_pred.append(predicted_label)


eval(y_true, y_pred)
