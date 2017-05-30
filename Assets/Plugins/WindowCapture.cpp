#include <Windows.h>
#include <d3d9.h>


extern "C" {
	__declspec(dllexport) int WINAPI Capture_ListTopLevelWindows(HWND *hwndarray, int maxcount);
	__declspec(dllexport) void WINAPI Capture_GetWindowSize(HWND hwnd, int *width, int *height);
	__declspec(dllexport) int WINAPI Capture_UpdateContent(HWND hwnd, unsigned char *pixels, int width, int height);
	__declspec(dllexport) void WINAPI Capture_SendMouseEvent(HWND hwnd, int kind, int x, int y);
	__declspec(dllexport) void WINAPI Capture_SendKeyEvent(int unichar);
}


typedef struct {
    HWND *hwndptr;
    int remaining;
    HWND desktop;
} enum_info_t;

static BOOL CALLBACK enum_window(HWND hwnd, LPARAM lParam)
{
    enum_info_t *info = (enum_info_t *)lParam;
    if (info->remaining > 0)
    {
        if (IsWindowVisible(hwnd) && hwnd != info->desktop)
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
    HWND desktop = GetDesktopWindow();
    enum_info_t info = { hwndarray, maxcount, desktop };
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
    int j;

	if (hwnd == INVALID_HANDLE_VALUE)
		return 0;


	LPDIRECT3D9 d3d = NULL;
	LPDIRECT3DDEVICE9 d3ddev = NULL;

	d3d = Direct3DCreate9(D3D_SDK_VERSION);
	if (d3d == NULL)
		return -1;

	D3DPRESENT_PARAMETERS d3dpp;
	ZeroMemory(&d3dpp, sizeof(d3dpp));
	d3dpp.Windowed = TRUE;
	d3dpp.SwapEffect = D3DSWAPEFFECT_DISCARD;
	d3dpp.BackBufferFormat = D3DFMT_UNKNOWN;
	d3dpp.BackBufferCount = 1;
	d3dpp.BackBufferWidth = width;
	d3dpp.BackBufferHeight = height;
	d3dpp.MultiSampleType = D3DMULTISAMPLE_NONE;
	d3dpp.MultiSampleQuality = 0;
	d3dpp.EnableAutoDepthStencil = TRUE;
	d3dpp.AutoDepthStencilFormat = D3DFMT_D16;
	d3dpp.hDeviceWindow = hwnd;
	d3dpp.Flags = D3DPRESENTFLAG_LOCKABLE_BACKBUFFER;
	d3dpp.FullScreen_RefreshRateInHz = D3DPRESENT_RATE_DEFAULT;
	d3dpp.PresentationInterval = D3DPRESENT_INTERVAL_DEFAULT;

	d3d->CreateDevice(D3DADAPTER_DEFAULT,
		D3DDEVTYPE_HAL,
		hwnd,
		D3DCREATE_HARDWARE_VERTEXPROCESSING,
		&d3dpp,
		&d3ddev);
	if (d3ddev == NULL)
		return -2;

	//IDirect3DSurface9* pRenderTarget = NULL;
	IDirect3DSurface9* pDestTarget = NULL;
	//d3ddev->GetRenderTarget(0, &pRenderTarget);
	//if (pRenderTarget == NULL)
	//	return -3;


	//d3ddev->GetRenderTargetData(pRenderTarget, pDestTarget);
	//d3ddev->GetFrontBufferData(0, pDestTarget);
	//IDirect3DSurface9* pBackBuffer = NULL;
	d3ddev->GetBackBuffer(0, 0, D3DBACKBUFFER_TYPE_MONO, &pDestTarget);
	/*if (pRenderTarget == NULL)
		return -3;

	D3DSURFACE_DESC sd;
	pRenderTarget->GetDesc(&sd);

	d3ddev->CreateOffscreenPlainSurface(sd.Width, sd.Height, sd.Format, D3DPOOL_SYSTEMMEM, &pDestTarget, NULL);*/
	if (pDestTarget == NULL)
		return -4;

	/*d3ddev->GetRenderTargetData(pRenderTarget, pDestTarget);*/

	D3DLOCKED_RECT lockedRect;
	lockedRect.pBits = NULL;
	pDestTarget->LockRect(&lockedRect, NULL, D3DLOCK_NO_DIRTY_UPDATE | D3DLOCK_NOSYSLOCK | D3DLOCK_READONLY);
	if (lockedRect.pBits == NULL)
		return -6;

	for (j = 0; j < height; j++)
	{
		const char *src = ((const char *)lockedRect.pBits) + j * lockedRect.Pitch;
		unsigned char *dst = pixels + (height - 1 - j) * width * 4;
		memcpy(dst, src, width * 4);
	}
	pDestTarget->UnlockRect();

	pDestTarget->Release();
	//pRenderTarget->Release();
	d3ddev->Release();
	d3d->Release();
	return 1;


#if 0
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

#if 1
    BitBlt(hFDC, 0, 0, width, height, hVDC, 0, 0, SRCCOPY);
#else
    PrintWindow(hwnd, hFDC, PW_CLIENTONLY);
#endif
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
#endif
}

/*__declspec(dllexport)
void WINAPI Capture_SendMouseEvent(HWND hwnd, int kind, int x, int y)
{
    int message, wparam;
    LPARAM lparam;
    POINT pt = { x, y };
    while (1)
    {
        HWND hwnd1 = ChildWindowFromPointEx(hwnd, pt, CWP_SKIPINVISIBLE);
        if (hwnd1 == NULL || hwnd1 == hwnd)
            break;
        MapWindowPoints(hwnd, hwnd1, &pt, 1);
        hwnd = hwnd1;
    }

    message = 0;
    wparam = 0;
    lparam = MAKELPARAM(pt.x, pt.y);
    switch (kind)
    {
        case 1: message = WM_LBUTTONDOWN; wparam = MK_LBUTTON; break;
        case 2: message = WM_MOUSEMOVE;   wparam = MK_LBUTTON; break;
        case 3: message = WM_LBUTTONUP;                        break;
    }
    SendMessage(hwnd, message, wparam, lparam); 
}*/

__declspec(dllexport)
void WINAPI Capture_SendMouseEvent(HWND hwnd, int kind, int x, int y)
{
    INPUT input;
    POINT pt = { x, y };
    if (ClientToScreen(hwnd, &pt) == 0)
        return;

    ZeroMemory(&input, sizeof(input));
    input.type = INPUT_MOUSE;
    input.mi.dx = pt.x * 65536 / GetSystemMetrics(SM_CXSCREEN);
    input.mi.dy = pt.y * 65536 / GetSystemMetrics(SM_CYSCREEN);
    input.mi.dwFlags = MOUSEEVENTF_ABSOLUTE | MOUSEEVENTF_MOVE;
    SendInput(1, &input, sizeof(INPUT));

    switch (kind)
    {
        case 1:  /* mouse ldown */
            input.mi.dwFlags = MOUSEEVENTF_LEFTDOWN;
            break;

        case 3:  /* mouse lup */
            input.mi.dwFlags = MOUSEEVENTF_LEFTUP;
            break;

        default:
            return;   /* done */
    }
    SendInput(1, &input, sizeof(INPUT));
}

__declspec(dllexport)
void WINAPI Capture_SendKeyEvent(int unichar)
{
    INPUT input;
    ZeroMemory(&input, sizeof(input));
    input.type = INPUT_KEYBOARD;

    if (unichar > 0)
    {
        input.ki.wScan = unichar;
        input.ki.dwFlags = KEYEVENTF_UNICODE;
    }
    else
    {
        int scan;
        switch (unichar) {
        case -1: scan = 14; break;   /* backspace */
        case -2: scan = 28; break;   /* enter */
        case -3: scan = 1;  break;   /* esc */
        case -4: scan = 15; break;   /* tab */
        default: return;
        }
        input.ki.wScan = scan;
        input.ki.dwFlags = KEYEVENTF_SCANCODE;
    }
    SendInput(1, &input, sizeof(INPUT));

    input.ki.dwFlags |= KEYEVENTF_KEYUP;
    SendInput(1, &input, sizeof(INPUT));
}
