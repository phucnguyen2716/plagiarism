# Marking Engine

An AI-powered marking system that automates grading of student PDF submissions with plagiarism detection.

## Features

- **PDF Upload**: Upload student submissions with automatic text extraction
- **AI Grading**: Google Gemini-powered grading with detailed feedback
- **Plagiarism Detection**: Built-in similarity checking across submissions
- **Dashboard**: View and manage all submissions in one place

## Prerequisites

- Python 3.8+
- Node.js and npm
- Google Gemini API key

## Quick Start

### 1. Clone and Setup

```bash
git clone https://github.com/dqduong2003/marking-engine.git
cd marking-engine
```

### 2. Configure Environment

Create `backend/.env`:

```bash
GEMINI_API_KEY=your_gemini_api_key_here
```

### 3. Install Dependencies

**Backend:**
```bash
cd backend
pip install -r requirements.txt
```

**Frontend:**
```bash
cd frontend
npm install
```

### 4. Run the Application

**Terminal 1 - Backend:**
```bash
cd backend
uvicorn app.main:app --reload
```

**Terminal 2 - Frontend:**
```bash
cd frontend
npm run dev
```

### 5. Access the App

- Frontend: http://localhost:5173
- API Docs: http://localhost:8000/docs

## Usage

1. Upload PDFs with student IDs in the filename (e.g., `S10485739_Alice.pdf`)
2. View submissions in the dashboard
3. Click "Grade All" or grade individual submissions
4. Review AI-generated scores and feedback

## Project Structure

```
marking-engine/
├── backend/          # FastAPI + Google Gemini
│   ├── app/
│   │   ├── routers/  # API endpoints
│   │   └── services/ # Grading & plagiarism
│   └── requirements.txt
└── frontend/         # React + Vite + TypeScript
    └── src/
```

## Screenshots
### Dashboard
![Dashboard](/screenshots/dashboard.png)

## Troubleshooting

| Issue | Solution |
|-------|----------|
| Missing API key | Add `GEMINI_API_KEY` to `backend/.env` |
| PDF parsing fails | Use PDFs with selectable text (not scanned images) |
| Invalid filename | Use format: `S{student_id}_{name}.pdf` |
| Port conflicts | Change ports in backend/frontend configs |

## Tech Stack

- **Backend**: Python, FastAPI, Google Gemini API, PyPDF2
- **Frontend**: React, TypeScript, Vite
- **Storage**: JSON file-based