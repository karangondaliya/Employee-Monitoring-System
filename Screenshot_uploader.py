import time
import requests
import pyautogui
from io import BytesIO
from PIL import Image

# API Endpoint
UPLOAD_URL = "https://localhost:7227/api/Screenshots/UploadFile"  # Replace with your actual API

# Your authentication token (replace with a valid one)
AUTH_TOKEN = "your_token_here"  # Fetch from .NET MAUI SecureStorage if needed

def capture_screenshot():
    """Capture a screenshot and return it as a byte stream."""
    screenshot = pyautogui.screenshot()
    image_bytes = BytesIO()
    screenshot.save(image_bytes, format="PNG")
    image_bytes.seek(0)
    return image_bytes

def upload_screenshot():
    """Capture and upload a screenshot to the API."""
    image_stream = capture_screenshot()

    files = {'imageFile': ('screenshot.png', image_stream, 'image/png')}
    headers = {'Authorization': f'Bearer {AUTH_TOKEN}'}

    response = requests.post(UPLOAD_URL, files=files, headers=headers)

    if response.status_code == 201 or response.status_code == 200:
        print("✅ Screenshot uploaded successfully.")
    else:
        print(f"❌ Upload failed: {response.status_code} - {response.text}")

# Run every 5 minutes
if __name__ == "__main__":
    print("📷 Screenshot capturing started...")
    while True:
        upload_screenshot()
        time.sleep(300)  # Wait 5 minutes before capturing next screenshot
