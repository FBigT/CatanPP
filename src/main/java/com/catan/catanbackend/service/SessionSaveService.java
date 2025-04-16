package com.catan.catanbackend.service;

import com.catan.catanbackend.model.Session;
import com.catan.catanbackend.model.SessionSave;
import com.catan.catanbackend.repository.SessionSaveRepository;
import org.springframework.stereotype.Service;

import java.util.List;
import java.util.Optional;

@Service
public class SessionSaveService {
    private final SessionSaveRepository repository;
    private final SessionService sessionService;

    public SessionSaveService(SessionSaveRepository repository, SessionService sessionService) {
        this.repository = repository;
        this.sessionService = sessionService;
    }

    public SessionSave save(String saveName, Session session) {
        return repository.saveAndFlush(new SessionSave(saveName, session, session.getTurnNumber(), sessionService.createSaveJson(session)));
    }

    public Optional<SessionSave> findById(Long id){
        return repository.findById(id);
    }

    public void deleteSave(Long saveId) {
        repository.deleteById(saveId);
        repository.flush();
    }

    public List<SessionSave> getSavesByHostId(Long hostId) {
        return repository.findBySessionHostId(hostId);
    }
}
