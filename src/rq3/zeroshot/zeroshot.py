import json
from pathlib import Path
from transformers import pipeline
from sklearn.metrics import classification_report

# Load data
def load_json_array(path):
    with open(path, encoding='utf-8') as f:
        return json.load(f)

# Configure
DATA_PATH = "dataset700.json"  # Modify the path accordingly
LABEL_FIELD = "Label"
TEXT_FIELDS = ["RawMessage", "Changes"]

candidate_labels = [
    "Bug Fix",
    "Functional Improvement",
    "NonFunctional Enhancement",
    "Documentation",
    "Test"
]

# Init zero-shot classifier
classifier = pipeline("zero-shot-classification", model="facebook/bart-large-mnli")

# Load dataset
data = load_json_array(DATA_PATH)

# Prediction loop
y_true = []
y_pred = []

for rec in data:
    if not all(k in rec for k in TEXT_FIELDS + [LABEL_FIELD]):
        continue

    text = f"{rec['RawMessage']}\n\n{rec['Changes']}"
    true_label = rec[LABEL_FIELD]

    result = classifier(text, candidate_labels, multi_label=False)
    predicted_label = result["labels"][0]

    y_true.append(true_label)
    y_pred.append(predicted_label)

# Evaluate
report = classification_report(y_true, y_pred, labels=candidate_labels, zero_division=0)
print("Classification Report:\n")
print(report)
