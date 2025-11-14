from sklearn.metrics import precision_score, recall_score, f1_score
from sklearn.metrics import classification_report


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
