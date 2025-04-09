package com.catan.catanbackend.service;

import com.catan.catanbackend.model.Session;
import com.catan.catanbackend.model.SessionSave;
import com.catan.catanbackend.repository.SessionSaveRepository;
import org.springframework.stereotype.Service;

import java.util.List;

@Service
public class SessionSaveService {
    private final SessionSaveRepository repository;

    public SessionSaveService(SessionSaveRepository repository) {
        this.repository = repository;
    }

    public SessionSave save(String saveName, Session session) {
        return repository.save(new SessionSave(saveName, session, session.getTurnNumber()));
    }

    public List<SessionSave> getSavesByHostId(Long hostId) {
        return repository.findBySessionHostId(hostId);
    }
}
