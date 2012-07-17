﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BizHawk.Emulation.Consoles.Nintendo
{
	class Mapper230 : NES.NESBoardBase
	{
		/*
			* Here are Disch's original notes:  
		========================
		=  Mapper 230          =
		========================

		Example Game:
		--------------------------
		22-in-1


		Reset Driven:
		---------------------------
		The mapper has 2 main modes:  Contra mode, and multicart mode.  Performing a Soft Reset switches between
		them.


		Notes:
		---------------------------

		This multicart has an odd PRG size (not power of 2).  This is because there are 2 PRG chips.
		The first is 128k and contains Contra, the other is 512k and contains the multicart.

		A soft reset changes which chip is used as well as other stuff relating to the mode


		Registers:
		---------------------------


		Contra Mode $8000-FFFF:     [.... .PPP]

		Multicart Mode $8000-FFFF:  [.MOP PPPP]

		M = Mirroring (0=Horz, 1=Vert)
		O = PRG Mode
		P = PRG Page

		Note:
		Mirroring is always Vert in Contra Mode.



		PRG Setup:
		---------------------------

						$8000   $A000   $C000   $E000  
						+---------------+---------------+
		Contra Mode:    |     $8000     |     { 7 }     |  <---  use chip 0
						+-------------------------------+
		Multi Mode 0:   |             <Reg>             |  <---  use chip 1
						+-------------------------------+
		Multi Mode 1:   |      Reg      |      Reg      |  <---  use chip 1
						+---------------+---------------+
 
 
		chip 0 = 128k PRG  (offset 0x00010-0x2000F)
		chip 1 = 512k PRG  (offset 0x20010-0xA000F)
		*/

		public int prg_page;
		public bool prg_mode;
		public bool contra_mode;
		public int prg_bank_mask_16k;

		public override bool Configure(NES.EDetectionOrigin origin)
		{
			switch (Cart.board_type)
			{
				case "MAPPER230":
					break;
				default:
					return false;
			}
			contra_mode = true;
			SetMirrorType(EMirrorType.Vertical);
			prg_mode = false;
			prg_bank_mask_16k = Cart.prg_size / 16 - 1;
			return true;
		}

		public override void SyncState(Serializer ser)
		{
			ser.Sync("contra_mode", ref contra_mode);
			ser.Sync("prg_page", ref prg_page);
			ser.Sync("prg_mode", ref prg_mode);
			base.SyncState(ser);
		}

		public override void WritePRG(int addr, byte value)
		{
			if (contra_mode)
			{
				prg_page = value & 0x07;
			}
			else
			{
				prg_page = value & 0x0F;
				prg_mode = value.Bit(5);
				
				if (addr.Bit(6))
				{
					SetMirrorType(EMirrorType.Horizontal);
				}
				else
				{
					SetMirrorType(EMirrorType.Vertical);
				}
			}
		}

		public override byte ReadPRG(int addr)
		{
			if (contra_mode)
			{
				if (addr < 0x4000)
				{
					return ROM[((prg_page & prg_bank_mask_16k) * 0x4000) + addr];
				}
				else
				{
					return ROM[(7 * 0x4000) + addr - 0x4000];
				}
			}
			else
			{
				if (prg_mode)
				{
					return ROM[(prg_page * 0x8000) + addr]; //TODO
				}
				else
				{
					if (addr < 0x4000)
					{
						return ROM[((prg_page & prg_bank_mask_16k) * 0x4000) + addr];
					}
					else
					{
						return ROM[((prg_page & prg_bank_mask_16k) * 0x4000) + (addr - 0x4000)];
					}
					
				}
			}
		}
	}
}
