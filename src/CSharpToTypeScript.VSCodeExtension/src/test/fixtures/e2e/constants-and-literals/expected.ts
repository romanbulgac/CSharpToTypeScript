export interface Document {
    type: string;
}

export interface Album {
    role: "Admin";
    id: number;
    type: "album";
}

export interface Track {
    id: number;
    type: "track";
}
