# Backporting-ROS

A research project focused on identifying and analyzing commit backporting patterns in open-source repositories using machine learning and data analysis techniques.

## Overview

This project investigates Research Questions (RQ) related to commit backporting—the process of applying changes from one branch or version to another. The repository contains tools and datasets for collecting commit data, analyzing backporting patterns, and building machine learning models to predict whether commits are backported.
## Key Components

### RQ1: Data Collection Tool (C#)

Located in `src/rq1/data-collector-CS/`, this C# application is responsible for:
- Collecting commits from repositories
- Labeling commits as **backported** or **not backported**
- Building the initial dataset for analysis

**Files:**
- `Program.cs` - Main execution logic
- `Commit.cs` - Commit data structures and operations
- `Repository.cs` - Repository interaction and querying
- `Extensions.cs` - Helper extension methods

### RQ2: Dataset Analysis & Fine-tuning Tool (C#)

Located in `src/rq2/finetune-dataset-analysis-tool/`, this tool handles:

1. **Dataset Preparation**: Takes the large collected dataset and:
   - Splits data into training, validation, and test sets
   - Formats data for OpenAI fine-tuning models
   - Exports datasets in required formats

2. **Model Analysis**: Evaluates GPT model accuracy on backporting predictions

3. **Temporal Analysis**: Analyzes average time differences between:
   - Original commits
   - Corresponding backported commits

### RQ3: Datasets

The `data/rq3/` directory contains prepared datasets with 700 samples each:
- **train700.jsonl** - Training dataset
- **valid700.jsonl** - Validation dataset  
- **test700.jsonl** - Test dataset
- **dataset700.json** - Complete dataset

### Python Utilities

**`src/utils/model_eval.py`** - Model evaluation module providing:
- Precision calculation
- Recall calculation
- F1-score computation
- Detailed classification reports

## Technologies

- **Languages**: C#, Python
- **ML/Data**: scikit-learn, OpenAI API
- **Data Format**: JSONL, JSON

## Usage

### Running the Data Collector (C#)
1. Import the C# files into your IDE (Visual Studio, Rider, etc.)
2. Configure repository paths and authentication
3. Execute `Program.cs` to collect and label commit data

### Running the Analysis Tool (C#)
1. Prepare your dataset with the data collector
2. Import the finetune tool into your C# IDE
3. Configure input/output paths
4. Run to generate training datasets and analyze results

### Model Evaluation (Python)
```python
from src.utils.model_eval import eval

# Evaluate predictions
precision, recall, f1 = eval(y_true, y_pred)
