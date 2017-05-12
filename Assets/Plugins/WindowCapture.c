#include <Windows.h>


typedef struct {
    HWND *hwndptr;
    int remaining;
} enum_info_t;

static BOOL CALLBACK enum_window(HWND hwnd, LPARAM lParam)
{
    enum_info_t *info = (enum_info_t *)lParam;
    if (info->remaining > 0)
    {
        if (IsWindowVisible(hwnd))
        {
            *info->hwndptr++ = hwnd;
            info->remaining--;
        }
        return TRUE;
    }
    else
        return FALSE;
}

__declspec(dllexport)
int WINAPI Capture_ListTopLevelWindows(HWND *hwndarray, int maxcount)
{
    enum_info_t info = { hwndarray, maxcount };
    EnumWindows(enum_window, (LPARAM)&info);
    return maxcount - info.remaining;
}

__declspec(dllexport)
void WINAPI Capture_GetWindowSize(HWND hwnd, int *width, int *height)
{
    RECT rect;
    if (GetClientRect(hwnd, &rect) != 0)
    {
        *width = rect.right;
        *height = rect.bottom;
    }
    else
    {
        *width = *height = -1;
    }
}

__declspec(dllexport)
int WINAPI Capture_UpdateContent(HWND hwnd, unsigned char *pixels,
                                 int width, int height)
{
    HDC hVDC = NULL, hFDC = NULL;
    HBITMAP hbmBitmap = NULL;
    int result = 0;
    BYTE *pBitmapBits;
    BITMAPINFO bmi;
    int i;

    if (hwnd == INVALID_HANDLE_VALUE)
        goto error;

    hVDC = GetDC(hwnd);
    if (hVDC == NULL)
        goto error;

    // Prepare to create a bitmap

    ZeroMemory( &bmi.bmiHeader,  sizeof(BITMAPINFOHEADER) );
    bmi.bmiHeader.biSize        = sizeof(BITMAPINFOHEADER);
    bmi.bmiHeader.biWidth       = width;
    bmi.bmiHeader.biHeight      = height;
    bmi.bmiHeader.biPlanes      = 1;
    bmi.bmiHeader.biCompression = BI_RGB;
    bmi.bmiHeader.biBitCount    = 32;

    // Create and clear a DC and a bitmap for the font

    hFDC       = CreateCompatibleDC( hVDC );
    if (hFDC == NULL)
        goto error;

    hbmBitmap = CreateDIBSection( hFDC, &bmi, DIB_RGB_COLORS,
                                  (VOID**)&pBitmapBits, NULL, 0 );
    if (hbmBitmap == NULL)
        goto error;

    SelectObject( hFDC, hbmBitmap );
    SetMapMode(hFDC, GetMapMode(hVDC));

    BitBlt(hFDC, 0, 0, width, height, hVDC, 0, 0, SRCCOPY);
    GdiFlush();

    for (i = 0; i < width * height * 4; i += 4)
    {
        pixels[i + 0] = pBitmapBits[i + 2];
        pixels[i + 1] = pBitmapBits[i + 1];
        pixels[i + 2] = pBitmapBits[i + 0];
        pixels[i + 3] = 255;
    }
  
    result = 1;
error:
    if (hbmBitmap != NULL)
        DeleteObject(hbmBitmap);
    if (hFDC != NULL)
        DeleteDC(hFDC);
    if (hVDC != NULL)
        ReleaseDC(hwnd, hVDC);
    return result;
}
