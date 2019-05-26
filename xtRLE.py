import argparse, os

XTRLE_MAGIC = 'xtRLE'
KEY_ONE = 0xA5
KEY_TWO = 0x00

def bytes_to_int(arr):
    size = len(arr)
    res = 0
    for i in range(0, 4):
        res |= (int(arr[i]) << ((size - 1 - i) * 8))
    return res

def decompress(file):
    magic = file.read(len(XTRLE_MAGIC))
    if magic.decode('ascii') != XTRLE_MAGIC:
        file.close()
        raise ValueError('Infile is not encoded with {}'.format(XTRLE_MAGIC))

    # skip magic end (0x00)
    file.read(1)
    # unknown value
    file.read(4)

    # recover size
    size = bytes_to_int(file.read(4)[::-1])

    data = [0] * size
    data_ptr = 0

    c = file.read(1)
    while c and c[0] != 0x0:
        if c[0] == 0x0:
            file.seek(0x0E + num)
        else:
            num = c[0] & 0x7F

            if c[0] & 0x80:
                # read whole num-chunk from file
                new_data = file.read(num)
                if not new_data:
                    file.close()
                    raise ValueError('Error reading input file')
                else:
                    for i in range(0, num):
                        data[data_ptr + i] = new_data[i]
            else:
                # copy next byte num-times
                c = file.read(1)
                if c[0] == 0x0:
                    file.close()
                    raise ValueError('Error reading input file')
                for i in range(0, num):
                    data[data_ptr + i] = c[0]

            data_ptr += num
        c = file.read(1)

    return data, size

def decrypt(data, size):
    if not size:
        return

    res = data.copy()
    c = data[0]
    i = 1

    while True:
        d = res[i]
        if d == c:
            while i < size:
                d = res[i]
                res[i - 1] = c ^ KEY_TWO
                if d != c:
                    break
                i += 1
        else:
            res[i - 1] = c ^ KEY_ONE

        if i >= size - 1:
            res[size - 1] = d ^ (KEY_TWO if c == d else KEY_ONE)
            break

        c = d
        i += 1

    return res

def run(key1, key2, infile, outfile):
    if infile == outfile:
        raise ValueError('Input and output files are the same')

    if not os.path.exists(infile):
        raise FileNotFoundError("Input file does not exist")
    f_in = open(infile, 'rb')

    if not outfile:
        outfile = './' + infile.split('.')[0][::-1].split('/')[0][::-1] + '.csv'
        print('Output not given, using {}'.format(outfile))
        f_out = open(outfile, 'w')
    else:
        if os.path.exists(outfile):
            os.remove(outfile)
            print('Overwriting {}'.format(outfile))
        f_out = open(outfile, 'w')

    data, size = decompress(f_in)
    if not data:
        raise ValueError('Decompressed file appears to be empty')

    res = decrypt(data, size)
    for x in res:
        f_out.write(chr(x))

    f_in.close()
    f_out.close()

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="xtRLE decoder")
    parser.add_argument('--key1', '-k1', type=int, required=False, help='Key 1')
    parser.add_argument('--key2', '-k2', type=int, required=False, help='Key 2')
    parser.add_argument('--infile', '-i', type=str, required=True, help='Input file')
    parser.add_argument('--outfile', '-o', type=str, required=False, help='Output file')
    args = parser.parse_args()

    run(args.key1, args.key2, args.infile, args.outfile)

