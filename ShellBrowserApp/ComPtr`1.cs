// Copyright (c) Files Community
// Licensed under the MIT License.

using System;
using System.Runtime.CompilerServices;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;

namespace Windows.Win32
{
	/// <summary>
	/// Contains a COM pointer and a set of methods to work with the pointer safely.
	/// </summary>
	public unsafe struct ComPtr<T> : IDisposable where T : unmanaged, IComIID
	{
		private T* _ptr;

		public readonly bool IsNull
			=> _ptr == null;

		public ComPtr(T* ptr)
		{
			_ptr = ptr;

			if (ptr is not null)
				((IUnknown*)ptr)->AddRef();
		}

		public void Attach(T* other)
		{
			if (_ptr is not null)
				((IUnknown*)_ptr)->Release();

			_ptr = other;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly T* Get()
		{
			return _ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly T** GetAddressOf()
		{
			return (T**)Unsafe.AsPointer(ref Unsafe.AsRef(in this));
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly ComPtr<U> As<U>() where U : unmanaged, IComIID
		{
			ComPtr<U> ptr = default;
			HRESULT hr = ((IUnknown*)_ptr)->QueryInterface((Guid*)Unsafe.AsPointer(ref Unsafe.AsRef(in U.Guid)), (void**)ptr.GetAddressOf());
			return ptr;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly HRESULT CoCreateInstance<U>(CLSCTX dwClsContext = CLSCTX.CLSCTX_LOCAL_SERVER)
		{
			Guid iid = typeof(T).GUID, clsid = typeof(U).GUID;
			return PInvoke.CoCreateInstance(&clsid, null, dwClsContext, &iid, (void**)this.GetAddressOf());
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose()
		{
			T* ptr = _ptr;
			if (ptr is not null)
			{
				_ptr = null;
				((IUnknown*)ptr)->Release();
			}
		}
	}
}
